import { Injectable, ErrorHandler, Injector } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { Logger } from '../logger.service';
import { NotificationService } from '../../shared/notification.service';
import { AppInsightsService } from 'src/app/app-insights.service';

@Injectable()
export class GlobalErrorHandler implements ErrorHandler {

  // Error handling is important and needs to be loaded first.
  // Because of this we should manually inject the services with Injector.
  constructor(private injector: Injector) { }

  handleError(error: Error | HttpErrorResponse) {

    // Log errors to Application insights
    this.injector.get<AppInsightsService>(AppInsightsService).logException(error);
    console.log(error);
    
    const logger = this.injector.get(Logger);
    const notifier = this.injector.get(NotificationService);

    let errorMessage;
    let stackTrace;

    if (error instanceof HttpErrorResponse) {
      // Server Error
      errorMessage = this.getServerMessage(error);
      notifier.showError(errorMessage);
    } else {
      // Chunk failed
      const chunkFailedMessage = /Loading chunk [\d]+ failed/;
      if (chunkFailedMessage.test(error.message)) {
        window.location.reload();
      }
      // Client Error
      errorMessage = this.getClientMessage(error);
      stackTrace = this.getClientStack(error);

      if(!stackTrace || !errorMessage) 
        return
      
      // TODO: have not found any solution to handle arrow-key down press exception when user done selecting the
      // resources on quick-add popup, hence once the solution is found need to remove the below error handling
      const typeaheadDownArrowKeyErrorMessage = 'Cannot read property \'disabled\' of undefined';
      if (stackTrace.indexOf('_handleArrowDown') > -1 && errorMessage.indexOf(typeaheadDownArrowKeyErrorMessage) > -1) {
        return;
      }

      notifier.showError(errorMessage);
    }

    // logger.logExceptionInDatabase(stackTrace, errorMessage).subscribe(response => console.log(`Error logged to DB --> ${response}`));
    console.error(error);
  }

  getClientMessage(error: Error): string {
    if (!navigator.onLine) {
      return 'No Internet Connection';
    }
    return error.message ? error.message : error.toString();
  }

  getClientStack(error: Error): string {
    return error.stack;
  }

  getServerMessage(error: HttpErrorResponse): string {
    return error.message;
  }

  getServerStatusCode(error: HttpErrorResponse): string {
    return `Status:${error.status}, StatusText:${error.statusText} `;
  }
}
