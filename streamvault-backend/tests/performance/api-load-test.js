import http from 'k6/http';
import { check, sleep } from 'k6';
import { Rate } from 'k6/metrics';

// Custom metrics
const errorRate = new Rate('errors');

// Test configuration
export const options = {
  stages: [
    { duration: '2m', target: 100 }, // Ramp up to 100 users
    { duration: '5m', target: 100 }, // Stay at 100 users
    { duration: '2m', target: 200 }, // Ramp up to 200 users
    { duration: '5m', target: 200 }, // Stay at 200 users
    { duration: '2m', target: 300 }, // Ramp up to 300 users
    { duration: '5m', target: 300 }, // Stay at 300 users
    { duration: '2m', target: 0 },   // Ramp down
  ],
  thresholds: {
    http_req_duration: ['p(95)<500'], // 95% of requests must complete within 500ms
    http_req_failed: ['rate<0.1'],    // Error rate must be less than 10%
    errors: ['rate<0.1'],             // Custom error rate must be less than 10%
  },
};

const BASE_URL = 'http://localhost:5000/api';

// Test data
const users = [
  { email: 'user1@example.com', password: 'password123', tenant: 'tenant1' },
  { email: 'user2@example.com', password: 'password123', tenant: 'tenant2' },
  { email: 'user3@example.com', password: 'password123', tenant: 'tenant3' },
  { email: 'user4@example.com', password: 'password123', tenant: 'tenant4' },
  { email: 'user5@example.com', password: 'password123', tenant: 'tenant5' },
];

let authToken = null;

export function setup() {
  // Login to get auth token
  const loginResponse = http.post(`${BASE_URL}/auth/login`, JSON.stringify({
    email: users[0].email,
    password: users[0].password,
    tenantSlug: users[0].tenant,
  }), {
    headers: { 'Content-Type': 'application/json' },
  });

  if (loginResponse.status === 200) {
    const loginData = JSON.parse(loginResponse.body);
    authToken = loginData.accessToken;
  }

  return { authToken };
}

export default function(data) {
  const params = {
    headers: {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${data.authToken}`,
    },
  };

  // Test 1: Get Videos List
  const videosResponse = http.get(`${BASE_URL}/videos`, params);
  const videosSuccess = check(videosResponse, {
    'videos list status is 200': (r) => r.status === 200,
    'videos list response time < 200ms': (r) => r.timings.duration < 200,
    'videos list has data': (r) => JSON.parse(r.body).videos.length >= 0,
  });
  errorRate.add(!videosSuccess);

  // Test 2: Get Single Video
  if (videosResponse.status === 200) {
    const videos = JSON.parse(videosResponse.body).videos;
    if (videos.length > 0) {
      const videoId = videos[0].id;
      const videoResponse = http.get(`${BASE_URL}/videos/${videoId}`, params);
      const videoSuccess = check(videoResponse, {
        'video detail status is 200': (r) => r.status === 200,
        'video detail response time < 150ms': (r) => r.timings.duration < 150,
        'video detail has correct data': (r) => JSON.parse(r.body).id === videoId,
      });
      errorRate.add(!videoSuccess);
    }
  }

  // Test 3: Get Collections
  const collectionsResponse = http.get(`${BASE_URL}/collections`, params);
  const collectionsSuccess = check(collectionsResponse, {
    'collections list status is 200': (r) => r.status === 200,
    'collections list response time < 200ms': (r) => r.timings.duration < 200,
  });
  errorRate.add(!collectionsSuccess);

  // Test 4: Get Dashboard Stats
  const dashboardResponse = http.get(`${BASE_URL}/dashboard/stats`, params);
  const dashboardSuccess = check(dashboardResponse, {
    'dashboard stats status is 200': (r) => r.status === 200,
    'dashboard stats response time < 300ms': (r) => r.timings.duration < 300,
    'dashboard stats has required fields': (r) => {
      const data = JSON.parse(r.body);
      return data.totalVideos !== undefined && data.totalViews !== undefined;
    },
  });
  errorRate.add(!dashboardSuccess);

  // Test 5: Search Videos
  const searchResponse = http.get(`${BASE_URL}/videos?search=test&page=1&pageSize=20`, params);
  const searchSuccess = check(searchResponse, {
    'search videos status is 200': (r) => r.status === 200,
    'search videos response time < 250ms': (r) => r.timings.duration < 250,
  });
  errorRate.add(!searchSuccess);

  // Test 6: Upload Video (Simulated - just hit the endpoint)
  const uploadResponse = http.post(`${BASE_URL}/videos/init-upload`, JSON.stringify({
    fileName: 'test-video.mp4',
    fileSize: 1024000,
    mimeType: 'video/mp4',
  }), params);
  const uploadSuccess = check(uploadResponse, {
    'upload init status is 200': (r) => r.status === 200,
    'upload init response time < 500ms': (r) => r.timings.duration < 500,
  });
  errorRate.add(!uploadSuccess);

  sleep(1);
}

export function teardown(data) {
  // Cleanup if needed
  console.log('Performance test completed');
}
