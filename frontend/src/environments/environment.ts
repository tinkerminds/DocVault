export const environment = {
  production: true,
  apiBaseUrl: 'https://docvault-api-dev.azurewebsites.net/api',
  msalConfig: {
    clientId: '5cd72392-169b-4008-ace2-4a7b4d708a25',
    authority: 'https://login.microsoftonline.com/3997726f-031c-4e71-a9fe-bf8787c50dd8',
    redirectUri: 'https://yellow-river-03038cf00.4.azurestaticapps.net',
    scopes: ['api://5cd72392-169b-4008-ace2-4a7b4d708a25/access_as_user'],
  },
};
