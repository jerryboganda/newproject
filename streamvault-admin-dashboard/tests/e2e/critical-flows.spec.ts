import { test, expect } from '@playwright/test';

test.describe('Authentication Flow', () => {
  test.beforeEach(async ({ page }) => {
    // Navigate to the login page
    await page.goto('http://localhost:3000/login');
  });

  test('should allow user to login with valid credentials', async ({ page }) => {
    // Fill in login form
    await page.fill('[data-testid="email-input"]', 'test@example.com');
    await page.fill('[data-testid="password-input"]', 'password123');
    await page.fill('[data-testid="tenant-input"]', 'test-tenant');

    // Submit form
    await page.click('[data-testid="login-button"]');

    // Should redirect to dashboard
    await expect(page).toHaveURL('http://localhost:3000/dashboard');
    
    // Should show welcome message
    await expect(page.locator('[data-testid="welcome-message"]')).toBeVisible();
    await expect(page.locator('[data-testid="welcome-message"]')).toContainText('Welcome back');
  });

  test('should show error with invalid credentials', async ({ page }) => {
    // Fill in invalid credentials
    await page.fill('[data-testid="email-input"]', 'test@example.com');
    await page.fill('[data-testid="password-input"]', 'wrongpassword');
    await page.fill('[data-testid="tenant-input"]', 'test-tenant');

    // Submit form
    await page.click('[data-testid="login-button"]');

    // Should show error message
    await expect(page.locator('[data-testid="error-message"]')).toBeVisible();
    await expect(page.locator('[data-testid="error-message"]')).toContainText('Invalid credentials');
  });

  test('should allow user to register new account', async ({ page }) => {
    // Click on register tab
    await page.click('[data-testid="register-tab"]');

    // Fill in registration form
    await page.fill('[data-testid="register-email"]', 'newuser@example.com');
    await page.fill('[data-testid="register-password"]', 'password123');
    await page.fill('[data-testid="register-confirm-password"]', 'password123');
    await page.fill('[data-testid="register-first-name"]', 'John');
    await page.fill('[data-testid="register-last-name"]', 'Doe');
    await page.fill('[data-testid="register-tenant"]', 'new-tenant');

    // Submit form
    await page.click('[data-testid="register-button"]');

    // Should show verification message
    await expect(page.locator('[data-testid="verification-message"]')).toBeVisible();
    await expect(page.locator('[data-testid="verification-message"]')).toContainText('Please check your email');
  });

  test('should allow password reset flow', async ({ page }) => {
    // Click on forgot password
    await page.click('[data-testid="forgot-password-link"]');

    // Should navigate to forgot password page
    await expect(page).toHaveURL('http://localhost:3000/forgot-password');

    // Fill in email
    await page.fill('[data-testid="reset-email"]', 'test@example.com');

    // Submit form
    await page.click('[data-testid="send-reset-button"]');

    // Should show success message
    await expect(page.locator('[data-testid="success-message"]')).toBeVisible();
    await expect(page.locator('[data-testid="success-message"]')).toContainText('Password reset email sent');
  });

  test('should handle 2FA verification', async ({ page }) => {
    // First login with 2FA enabled user
    await page.fill('[data-testid="email-input"]', '2fauser@example.com');
    await page.fill('[data-testid="password-input"]', 'password123');
    await page.fill('[data-testid="tenant-input"]', 'test-tenant');

    // Submit form
    await page.click('[data-testid="login-button"]');

    // Should redirect to 2FA page
    await expect(page).toHaveURL('http://localhost:3000/2fa');

    // Fill in 2FA code
    await page.fill('[data-testid="2fa-code"]', '123456');

    // Verify code
    await page.click('[data-testid="verify-2fa-button"]');

    // Should redirect to dashboard
    await expect(page).toHaveURL('http://localhost:3000/dashboard');
  });
});

test.describe('Video Management Flow', () => {
  test.beforeEach(async ({ page }) => {
    // Login first
    await page.goto('http://localhost:3000/login');
    await page.fill('[data-testid="email-input"]', 'test@example.com');
    await page.fill('[data-testid="password-input"]', 'password123');
    await page.fill('[data-testid="tenant-input"]', 'test-tenant');
    await page.click('[data-testid="login-button"]');
    await page.waitForURL('http://localhost:3000/dashboard');
  });

  test('should upload video successfully', async ({ page }) => {
    // Navigate to upload page
    await page.click('[data-testid="upload-nav"]');
    await expect(page).toHaveURL('http://localhost:3000/dashboard/upload');

    // Select video file
    const fileInput = page.locator('[data-testid="file-input"]');
    await fileInput.setInputFiles('test-assets/sample-video.mp4');

    // Fill in video details
    await page.fill('[data-testid="video-title"]', 'Test Video Upload');
    await page.fill('[data-testid="video-description"]', 'This is a test video uploaded via E2E test');
    await page.selectOption('[data-testid="video-visibility"]', 'public');

    // Start upload
    await page.click('[data-testid="start-upload-button"]');

    // Should show upload progress
    await expect(page.locator('[data-testid="upload-progress"]')).toBeVisible();

    // Wait for upload to complete
    await expect(page.locator('[data-testid="upload-complete"]')).toBeVisible({ timeout: 30000 });

    // Should show success message
    await expect(page.locator('[data-testid="success-toast"]')).toBeVisible();
  });

  test('should view video list with filters', async ({ page }) => {
    // Navigate to videos page
    await page.click('[data-testid="videos-nav"]');
    await expect(page).toHaveURL('http://localhost:3000/dashboard/videos');

    // Should show video list
    await expect(page.locator('[data-testid="video-list"]')).toBeVisible();

    // Search for video
    await page.fill('[data-testid="search-input"]', 'Test');
    await expect(page.locator('[data-testid="video-item"]')).toHaveCount(1);

    // Filter by status
    await page.selectOption('[data-testid="status-filter"]', 'ready');
    await expect(page.locator('[data-testid="video-item"]')).toHaveCount(1);

    // Filter by visibility
    await page.selectOption('[data-testid="visibility-filter"]', 'public');
    await expect(page.locator('[data-testid="video-item"]')).toHaveCount(1);

    // Change to grid view
    await page.click('[data-testid="grid-view-button"]');
    await expect(page.locator('[data-testid="video-grid"]')).toBeVisible();
  });

  test('should edit video details', async ({ page }) => {
    // Navigate to videos page
    await page.click('[data-testid="videos-nav"]');

    // Click on first video
    await page.click('[data-testid="video-item"]:first-child');

    // Should be on video details page
    await expect(page.locator('[data-testid="video-details"]')).toBeVisible();

    // Click edit button
    await page.click('[data-testid="edit-video-button"]');

    // Update title
    await page.fill('[data-testid="video-title-input"]', 'Updated Test Video');
    await page.fill('[data-testid="video-description-input"]', 'Updated description');

    // Save changes
    await page.click('[data-testid="save-button"]');

    // Should show success message
    await expect(page.locator('[data-testid="success-message"]')).toBeVisible();

    // Verify updated title
    await expect(page.locator('[data-testid="video-title"]')).toContainText('Updated Test Video');
  });

  test('should delete video', async ({ page }) => {
    // Navigate to videos page
    await page.click('[data-testid="videos-nav"]');

    // Click on first video menu
    await page.click('[data-testid="video-menu"]:first-child');

    // Click delete option
    await page.click('[data-testid="delete-video-option"]');

    // Confirm deletion in modal
    await expect(page.locator('[data-testid="delete-modal"]')).toBeVisible();
    await page.click('[data-testid="confirm-delete-button"]');

    // Should show success message
    await expect(page.locator('[data-testid="success-toast"]')).toBeVisible();

    // Video should be removed from list
    await expect(page.locator('[data-testid="video-item"]')).toHaveCount(0);
  });
});

test.describe('Collection Management Flow', () => {
  test.beforeEach(async ({ page }) => {
    // Login first
    await page.goto('http://localhost:3000/login');
    await page.fill('[data-testid="email-input"]', 'test@example.com');
    await page.fill('[data-testid="password-input"]', 'password123');
    await page.fill('[data-testid="tenant-input"]', 'test-tenant');
    await page.click('[data-testid="login-button"]');
    await page.waitForURL('http://localhost:3000/dashboard');
  });

  test('should create new collection', async ({ page }) => {
    // Navigate to collections page
    await page.click('[data-testid="collections-nav"]');
    await expect(page).toHaveURL('http://localhost:3000/dashboard/collections');

    // Click create collection button
    await page.click('[data-testid="create-collection-button"]');

    // Fill in collection details
    await page.fill('[data-testid="collection-name"]', 'Test Collection');
    await page.fill('[data-testid="collection-description"]', 'This is a test collection');
    await page.selectOption('[data-testid="collection-visibility"]', 'public');

    // Create collection
    await page.click('[data-testid="save-collection-button"]');

    // Should show success message
    await expect(page.locator('[data-testid="success-message"]')).toBeVisible();

    // Collection should appear in list
    await expect(page.locator('[data-testid="collection-item"]')).toContainText('Test Collection');
  });

  test('should add videos to collection', async ({ page }) => {
    // Navigate to collections page
    await page.click('[data-testid="collections-nav"]');

    // Click on first collection
    await page.click('[data-testid="collection-item"]:first-child');

    // Click add videos button
    await page.click('[data-testid="add-videos-button"]');

    // Select videos to add
    await page.check('[data-testid="video-checkbox"]:first-child');
    await page.check('[data-testid="video-checkbox"]:nth-child(2)');

    // Add to collection
    await page.click('[data-testid="add-selected-button"]');

    // Should show success message
    await expect(page.locator('[data-testid="success-message"]')).toBeVisible();

    // Videos should appear in collection
    await expect(page.locator('[data-testid="collection-video"]')).toHaveCount(2);
  });

  test('should reorder videos in collection', async ({ page }) => {
    // Navigate to collections page
    await page.click('[data-testid="collections-nav"]');

    // Click on first collection
    await page.click('[data-testid="collection-item"]:first-child');

    // Enable reorder mode
    await page.click('[data-testid="reorder-button"]');

    // Drag first video to second position
    const firstVideo = page.locator('[data-testid="collection-video"]:first-child');
    const secondVideo = page.locator('[data-testid="collection-video"]:nth-child(2)');
    
    await firstVideo.dragTo(secondVideo);

    // Save order
    await page.click('[data-testid="save-order-button"]');

    // Should show success message
    await expect(page.locator('[data-testid="success-message"]')).toBeVisible();
  });
});

test.describe('Dashboard Analytics', () => {
  test.beforeEach(async ({ page }) => {
    // Login first
    await page.goto('http://localhost:3000/login');
    await page.fill('[data-testid="email-input"]', 'test@example.com');
    await page.fill('[data-testid="password-input"]', 'password123');
    await page.fill('[data-testid="tenant-input"]', 'test-tenant');
    await page.click('[data-testid="login-button"]');
    await page.waitForURL('http://localhost:3000/dashboard');
  });

  test('should display analytics dashboard', async ({ page }) => {
    // Should show quick stats
    await expect(page.locator('[data-testid="total-videos-stat"]')).toBeVisible();
    await expect(page.locator('[data-testid="total-views-stat"]')).toBeVisible();
    await expect(page.locator('[data-testid="storage-used-stat"]')).toBeVisible();
    await expect(page.locator('[data-testid="active-users-stat"]')).toBeVisible();

    // Should show charts
    await expect(page.locator('[data-testid="views-chart"]')).toBeVisible();
    await expect(page.locator('[data-testid="uploads-chart"]')).toBeVisible();
    await expect(page.locator('[data-testid="storage-chart"]')).toBeVisible();

    // Should show recent activity
    await expect(page.locator('[data-testid="recent-activity"]')).toBeVisible();
    const activityItems = page.locator('[data-testid="activity-item"]');
    await expect(activityItems).toHaveCount(1);
  });

  test('should filter analytics by date range', async ({ page }) => {
    // Click on date range filter
    await page.click('[data-testid="date-range-filter"]');

    // Select last 7 days
    await page.click('[data-testid="last-7-days"]');

    // Should update charts
    await expect(page.locator('[data-testid="views-chart"]')).toBeVisible();

    // Check if data is filtered (dates should be updated)
    const chartTitle = page.locator('[data-testid="chart-title"]');
    await expect(chartTitle).toContainText('Last 7 days');
  });

  test('should export analytics report', async ({ page }) => {
    // Click export button
    await page.click('[data-testid="export-button"]');

    // Select export format
    await page.selectOption('[data-testid="export-format"]', 'csv');

    // Download report
    const downloadPromise = page.waitForEvent('download');
    await page.click('[data-testid="download-report-button"]');
    const download = await downloadPromise;

    // Verify download
    expect(download.suggestedFilename()).toContain('analytics-report');
    expect(download.suggestedFilename()).toContain('.csv');
  });
});

test.describe('Super Admin Features', () => {
  test.beforeEach(async ({ page }) => {
    // Login as super admin
    await page.goto('http://localhost:3000/admin/login');
    await page.fill('[data-testid="email-input"]', 'admin@streamvault.com');
    await page.fill('[data-testid="password-input"]', 'adminpassword');
    await page.click('[data-testid="login-button"]');
    await page.waitForURL('http://localhost:3000/admin/dashboard');
  });

  test('should manage tenants', async ({ page }) => {
    // Navigate to tenants tab
    await page.click('[data-testid="tenants-tab"]');

    // Should show tenant list
    await expect(page.locator('[data-testid="tenant-list"]')).toBeVisible();

    // Search for tenant
    await page.fill('[data-testid="tenant-search"]', 'test');
    await expect(page.locator('[data-testid="tenant-item"]')).toHaveCount(1);

    // Click on tenant details
    await page.click('[data-testid="tenant-item"]:first-child');

    // Should show tenant details
    await expect(page.locator('[data-testid="tenant-details"]')).toBeVisible();
    await expect(page.locator('[data-testid="tenant-stats"]')).toBeVisible();
  });

  test('should create new tenant', async ({ page }) => {
    // Navigate to tenants tab
    await page.click('[data-testid="tenants-tab"]');

    // Click create tenant button
    await page.click('[data-testid="create-tenant-button"]');

    // Fill in tenant details
    await page.fill('[data-testid="tenant-name"]', 'New Test Tenant');
    await page.fill('[data-testid="tenant-slug"]', 'new-test-tenant');
    await page.fill('[data-testid="tenant-email"]', 'admin@newtenant.com');
    await page.fill('[data-testid="admin-first-name"]', 'Admin');
    await page.fill('[data-testid="admin-last-name"]', 'User');
    await page.fill('[data-testid="admin-email"]', 'admin@newtenant.com');
    await page.fill('[data-testid="admin-password"]', 'password123');

    // Create tenant
    await page.click('[data-testid="create-tenant-submit"]');

    // Should show success message
    await expect(page.locator('[data-testid="success-message"]')).toBeVisible();

    // New tenant should appear in list
    await expect(page.locator('[data-testid="tenant-item"]')).toContainText('New Test Tenant');
  });

  test('should suspend and unsuspend tenant', async ({ page }) => {
    // Navigate to tenants tab
    await page.click('[data-testid="tenants-tab"]');

    // Click on tenant menu
    await page.click('[data-testid="tenant-menu"]:first-child');

    // Click suspend option
    await page.click('[data-testid="suspend-tenant-option"]');

    // Enter suspension reason
    await page.fill('[data-testid="suspension-reason"]', 'Violation of terms');
    await page.click('[data-testid="confirm-suspend-button"]');

    // Should show success message
    await expect(page.locator('[data-testid="success-message"]')).toBeVisible();

    // Tenant should show as suspended
    await expect(page.locator('[data-testid="tenant-status"]')).toContainText('Suspended');

    // Unsuspend tenant
    await page.click('[data-testid="tenant-menu"]:first-child');
    await page.click('[data-testid="unsuspend-tenant-option"]');
    await page.click('[data-testid="confirm-unsuspend-button"]');

    // Should show success message
    await expect(page.locator('[data-testid="success-message"]')).toBeVisible();

    // Tenant should show as active
    await expect(page.locator('[data-testid="tenant-status"]')).toContainText('Active');
  });

  test('should impersonate tenant user', async ({ page }) => {
    // Navigate to tenants tab
    await page.click('[data-testid="tenants-tab"]');

    // Click on tenant menu
    await page.click('[data-testid="tenant-menu"]:first-child');

    // Click impersonate option
    await page.click('[data-testid="impersonate-option"]');

    // Should open new tab with impersonated session
    const newPage = await page.context().waitForEvent('page');
    await newPage.waitForLoadState();

    // Should be on tenant dashboard
    await expect(newPage).toHaveURL(/.*test-tenant\.streamvault\.app.*/);
    await expect(newPage.locator('[data-testid="impersonation-banner"]')).toBeVisible();
  });
});

test.describe('Error Handling', () => {
  test('should show 404 page for invalid routes', async ({ page }) => {
    // Navigate to invalid route
    await page.goto('http://localhost:3000/invalid-route');

    // Should show 404 page
    await expect(page.locator('[data-testid="404-page"]')).toBeVisible();
    await expect(page.locator('h1')).toContainText('Page not found');
  });

  test('should handle network errors gracefully', async ({ page }) => {
    // Intercept network requests and simulate error
    await page.route('**/api/videos', route => route.fulfill({
      status: 500,
      contentType: 'application/json',
      body: JSON.stringify({ error: 'Internal server error' })
    }));

    // Login first
    await page.goto('http://localhost:3000/login');
    await page.fill('[data-testid="email-input"]', 'test@example.com');
    await page.fill('[data-testid="password-input"]', 'password123');
    await page.fill('[data-testid="tenant-input"]', 'test-tenant');
    await page.click('[data-testid="login-button"]');
    await page.waitForURL('http://localhost:3000/dashboard');

    // Navigate to videos page
    await page.click('[data-testid="videos-nav"]');

    // Should show error message
    await expect(page.locator('[data-testid="error-message"]')).toBeVisible();
    await expect(page.locator('[data-testid="error-message"]')).toContainText('Failed to load videos');
  });

  test('should handle session timeout', async ({ page }) => {
    // Login first
    await page.goto('http://localhost:3000/login');
    await page.fill('[data-testid="email-input"]', 'test@example.com');
    await page.fill('[data-testid="password-input"]', 'password123');
    await page.fill('[data-testid="tenant-input"]', 'test-tenant');
    await page.click('[data-testid="login-button"]');
    await page.waitForURL('http://localhost:3000/dashboard');

    // Clear auth token to simulate session timeout
    await page.evaluate(() => {
      localStorage.removeItem('auth-storage');
    });

    // Try to access protected route
    await page.click('[data-testid="videos-nav"]');

    // Should redirect to login
    await expect(page).toHaveURL('http://localhost:3000/login');
    
    // Should show session timeout message
    await expect(page.locator('[data-testid="session-timeout-message"]')).toBeVisible();
  });
});
