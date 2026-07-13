module.exports = {
  '/api': {
    target: process.env['API_URL'] || 'http://localhost:5038',
    secure: false,
    changeOrigin: true,
    logLevel: 'info',
  },
};
