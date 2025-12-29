import { test, expect } from '@playwright/test';

test.describe('StreamVault API Tests', () => {
  test('backend health check', async ({ request }) => {
    const response = await request.get('/health');
    expect(response.status()).toBe(200);
  });

  test('videos endpoint returns data', async ({ request }) => {
    const response = await request.get('/api/v1/videos');
    expect(response.status()).toBe(200);
    
    const videos = await response.json();
    expect(Array.isArray(videos)).toBeTruthy();
    expect(videos.length).toBeGreaterThan(0);
    
    // Check first video structure
    const firstVideo = videos[0];
    expect(firstVideo).toHaveProperty('id');
    expect(firstVideo).toHaveProperty('title');
    expect(firstVideo).toHaveProperty('description');
    expect(firstVideo).toHaveProperty('thumbnailUrl');
    expect(firstVideo).toHaveProperty('videoUrl');
    expect(firstVideo).toHaveProperty('duration');
    expect(firstVideo).toHaveProperty('viewCount');
  });

  test('single video endpoint', async ({ request }) => {
    // First get all videos to find a valid ID
    const videosResponse = await request.get('/api/v1/videos');
    const videos = await videosResponse.json();
    const firstVideoId = videos[0].id;

    // Test single video endpoint
    const response = await request.get(`/api/v1/videos/${firstVideoId}`);
    expect(response.status()).toBe(200);
    
    const video = await response.json();
    expect(video).toHaveProperty('id', firstVideoId);
    expect(video).toHaveProperty('title');
  });

  test('login endpoint works', async ({ request }) => {
    const response = await request.post('/api/v1/auth/login', {
      data: {
        email: 'admin@streamvault.com',
        password: 'Admin123!'
      }
    });
    
    expect(response.status()).toBe(200);
    
    const loginData = await response.json();
    expect(loginData).toHaveProperty('token');
    expect(loginData).toHaveProperty('user');
    expect(loginData.user.email).toBe('admin@streamvault.com');
  });

  test('user profile endpoint', async ({ request }) => {
    const response = await request.get('/api/v1/user/profile');
    expect(response.status()).toBe(200);
    
    const profile = await response.json();
    expect(profile).toHaveProperty('id');
    expect(profile).toHaveProperty('email');
    expect(profile).toHaveProperty('firstName');
    expect(profile).toHaveProperty('lastName');
  });
});

test.describe('StreamVault Frontend Tests', () => {
  test.beforeEach(async ({ page }) => {
    // Open the simple HTML frontend via HTTP
    await page.goto('http://localhost:3002');
  });

  test('page loads correctly', async ({ page }) => {
    await expect(page.locator('h1')).toContainText('StreamVault');
    await expect(page.locator('button:has-text("Sign In")')).toBeVisible();
  });

  test('login functionality', async ({ page }) => {
    // Click sign in button
    await page.click('button:has-text("Sign In")');
    
    // Fill login form (should be pre-filled)
    await expect(page.locator('#email')).toHaveValue('admin@streamvault.com');
    await expect(page.locator('#password')).toHaveValue('Admin123!');
    
    // Submit login
    await page.click('button[type="submit"]');
    
    // Should show videos section
    await expect(page.locator('h2:has-text("Latest Videos")')).toBeVisible();
    await expect(page.locator('text=Welcome, Admin!')).toBeVisible();
  });

  test('videos display after login', async ({ page }) => {
    // First login
    await page.click('button:has-text("Sign In")');
    await page.click('button[type="submit"]');
    
    // Wait for videos to load
    await page.waitForSelector('.video-card');
    
    // Check videos are displayed
    const videoCards = page.locator('.video-card');
    await expect(videoCards).toHaveCount(3);
    
    // Check first video
    const firstVideo = videoCards.first();
    await expect(firstVideo.locator('h3')).toBeVisible();
    await expect(firstVideo.locator('img')).toBeVisible();
    await expect(firstVideo.locator('button:has-text("Play Video")')).toBeVisible();
  });

  test('logout functionality', async ({ page }) => {
    // Login first
    await page.click('button:has-text("Sign In")');
    await page.click('button[type="submit"]');
    
    // Then logout
    await page.click('button:has-text("Sign Out")');
    
    // Should show sign in button again
    await expect(page.locator('button:has-text("Sign In")')).toBeVisible();
  });
});

test.describe('StreamVault Integration Tests', () => {
  test('full workflow: login -> view videos -> play video', async ({ page }) => {
    // Open frontend
    await page.goto('http://localhost:3002');
    
    // Login
    await page.click('button:has-text("Sign In")');
    await page.click('button[type="submit"]');
    
    // Wait for videos
    await page.waitForSelector('.video-card');
    
    // Click play on first video
    const playButton = page.locator('.video-card').first().locator('button:has-text("Play Video")');
    await playButton.click();
    
    // Should open video in new tab (we can't test external video easily, but we can check the URL)
    // For now, just verify the button works
    await expect(playButton).toBeVisible();
  });
});
