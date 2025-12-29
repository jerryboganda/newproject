const express = require('express');
const cors = require('cors');

const app = express();
const port = 5000;

// Middleware
app.use(cors());
app.use(express.json());

// Health check
app.get('/api/v1/health', (req, res) => {
    res.json({ status: 'healthy', timestamp: new Date().toISOString() });
});

// Auth endpoints
app.post('/api/v1/auth/login', (req, res) => {
    const { email, password } = req.body;
    
    // Mock authentication
    if (email && password) {
        res.json({
            token: 'mock-jwt-token-' + Date.now(),
            user: {
                id: '1',
                email: email,
                role: email.includes('superadmin') ? 'SuperAdmin' : 'Admin',
                tenantId: 'tenant-1'
            }
        });
    } else {
        res.status(401).json({ error: 'Invalid credentials' });
    }
});

// User profile
app.get('/api/v1/user/profile', (req, res) => {
    res.json({
        id: '1',
        email: 'superadmin@streamvault.app',
        username: 'superadmin',
        firstName: 'Super',
        lastName: 'Admin',
        avatar: '',
        role: 'SuperAdmin',
        tenantId: null,
        isSuperAdmin: true,
        twoFactorEnabled: false
    });
});

// Dashboard stats
app.get('/api/v1/dashboard/stats', (req, res) => {
    res.json({
        totalVideos: 0,
        totalViews: 0,
        totalUsers: 1,
        totalStorage: 0,
        recentActivity: []
    });
});

// Videos endpoints
app.get('/api/v1/videos', (req, res) => {
    res.json([]);
});

app.post('/api/v1/videos', (req, res) => {
    res.json({ 
        id: 'video-' + Date.now(),
        ...req.body,
        status: 'uploaded'
    });
});

app.get('/api/v1/videos/:id', (req, res) => {
    res.json({ id: req.params.id, title: 'Sample Video' });
});

// Users endpoints
app.get('/api/v1/users', (req, res) => {
    res.json([{
        id: '1',
        email: 'superadmin@streamvault.app',
        role: 'SuperAdmin',
        createdAt: new Date().toISOString()
    }]);
});

// Settings endpoints
app.get('/api/v1/settings', (req, res) => {
    res.json({
        bunnyStream: {
            apiKey: '',
            libraryId: '',
            pullZoneId: ''
        },
        stripe: {
            publishableKey: '',
            secretKey: ''
        }
    });
});

app.put('/api/v1/settings', (req, res) => {
    res.json({ success: true, message: 'Settings updated' });
});

// Start server
app.listen(port, () => {
    console.log(`\n========================================`);
    console.log(`StreamVault Mock API Server`);
    console.log(`========================================`);
    console.log(`\nAPI is running at:`);
    console.log(`- http://localhost:${port}`);
    console.log(`- http://localhost:${port}/api/health`);
    console.log(`\nFrontend should connect automatically!`);
    console.log(`\nPress Ctrl+C to stop the server`);
    console.log(`========================================\n`);
});
