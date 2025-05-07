import { enableProdMode } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { AppModule } from './app/app.module';
import { environment } from './environments/environment';
import { environmentLoader as environmentLoaderPromise } from './environments/environmentLoader';

environmentLoaderPromise.then(env => {
  if (env.production) {
    enableProdMode();
  }

  environment.settings = env.settings;
  environment.name = env.name;
  environment.azureActiveDirectorySettings = env.azureActiveDirectorySettings;
  environment.resourcesApiConfig = env.resourcesApiConfig;
  
  if (env.name === 'UAT' || env.name === 'Production' ) {
    // Add scripts for Google analytics
    // const googleAnalyticsJqueryScript = document.createElement('script');
    // googleAnalyticsJqueryScript.type = 'text/javascript';
    // googleAnalyticsJqueryScript.src = environment.settings.googleAnalyticsJqueryUrl;
    // document.getElementsByTagName('body')[0].appendChild(googleAnalyticsJqueryScript);

    // const googleAnalyticsJsScript = document.createElement('script');
    // googleAnalyticsJsScript.type = 'text/javascript';
    // googleAnalyticsJsScript.src = environment.settings.googleAnalyticsJsUrl;
    // document.getElementsByTagName('body')[0].appendChild(googleAnalyticsJsScript);

    const googleAnalyticsProfileScript = document.createElement('script');
    googleAnalyticsProfileScript.type = 'text/javascript';
    googleAnalyticsProfileScript.src = environment.settings.googleAnalyticsProfileUrl;
    document.getElementsByTagName('body')[0].appendChild(googleAnalyticsProfileScript);
}

  platformBrowserDynamic().bootstrapModule(AppModule)
  .catch(err => console.error(err));
});

