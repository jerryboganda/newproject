const express = require('express');
const path = require('path');
const app = express();
const port = 3002;

// Serve the simple HTML frontend
app.get('/', (req, res) => {
  res.sendFile(path.join(__dirname, '../streamvault-simple.html'));
});

// Add CORS headers
app.use((req, res, next) => {
  res.header('Access-Control-Allow-Origin', '*');
  next();
});

app.listen(port, () => {
  console.log(`Frontend test server running at http://localhost:${port}`);
});
