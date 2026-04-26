const fs = require('fs');
const path = require('path');

const config = {
  DRAGONBALL_API_URL: process.env.DRAGONBALL_API_URL || 'http://localhost:5000',
  MUSIC_API_URL: process.env.MUSIC_API_URL || 'http://localhost:5002',
  KEYCLOAK_URL: process.env.KEYCLOAK_URL || 'http://localhost:8080',
};

const dir = path.join(__dirname, 'public');
if (!fs.existsSync(dir)) fs.mkdirSync(dir, { recursive: true });

fs.writeFileSync(
  path.join(dir, 'env-config.js'),
  `window.__ENV__ = ${JSON.stringify(config)};`
);
