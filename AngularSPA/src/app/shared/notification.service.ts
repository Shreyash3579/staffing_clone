import { Injectable } from '@angular/core';
import { ToastrService } from 'ngx-toastr';

export class ToastrMessage {
  message: string;
  title = '';
  messageType: string;
}

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  // -----------------------Local Variables--------------------------------------------//

  private notificationQueue: ToastrMessage[] = [];
  private validationMsgQueue: ToastrMessage[] = [];
  private isInfoShown = false;
  // private toastrQueue: ToastrMessage[] = [];

  constructor(private toastr: ToastrService) {
    this.toastr.toastrConfig.timeOut = 5000;
    this.toastr.toastrConfig.positionClass = 'toast-bottom-right';
    this.toastr.toastrConfig.autoDismiss = true;
    this.toastr.toastrConfig.closeButton = true;
    this.toastr.toastrConfig.newestOnTop = false;
    this.toastr.toastrConfig.disableTimeOut = false;
    this.toastr.toastrConfig.progressBar = true;
    this.toastr.toastrConfig.preventDuplicates = true;
    this.toastr.toastrConfig.iconClasses = {
      error: 'toast-error',
      info: 'toast-info',
      success: 'toast-success',
      warning: 'toast-warning'
    };
  }

  // -------------------Component LifeCycle Events and Functions----------------------//

  showSuccess(message, title: string = 'SUCCESS') {
    this.setNotificationQueue(message, title, 'success');
  }

  showStickySuccess(message, title: string = 'SUCCESS') {
    this.setNotificationQueue(message, title, 'stick-success');
  }

  showError(message, title: string = 'ERROR') {
    this.setNotificationQueue(message, title, 'error');
  }

  showWarning(message, title: string = 'VALIDATION') {
    this.setNotificationQueue(message, title, 'warning');
  }

  showInfo(message, title: string = 'INFO') {
    this.setNotificationQueue(message, title, 'info');
  }

  showCollection(toastrQueue) {
    const collectionToastr = this.toastr.info('Click to view notifications',
      'Show More', { disableTimeOut: true });
    collectionToastr.onShown.subscribe(notification => {
      this.isInfoShown = true;
      this.toastr.toastrConfig.disableTimeOut = true;
    });
    collectionToastr.onTap.subscribe(notification => {
      this.isInfoShown = false;
      this.toastr.toastrConfig.maxOpened = 0;
      this.showToastr(toastrQueue, false);
      this.notificationQueue = [];
    });
  }

  setNotificationQueue(message, title, messageType) {
    const toastrMessage = new ToastrMessage;
    toastrMessage.message = message;
    toastrMessage.title = title;
    toastrMessage.messageType = messageType;

    this.notificationQueue.push(toastrMessage);
    this.showNotifications(toastrMessage);
  }

  showNotifications(toastrMessage) {
    if (this.notificationQueue.length > 0 && this.notificationQueue.length <= 3 && !this.isInfoShown) {
      this.toastr.toastrConfig.maxOpened = 3;
      this.toastr.toastrConfig.timeOut = 5000;

      switch (toastrMessage.messageType) {
        case ('success'): {
          this.toastr.success(toastrMessage.message, toastrMessage.title, { disableTimeOut: false }).onHidden.subscribe(notification => {
            this.notificationQueue.pop();
          });
          break;
        }
        case ('stick-success'): {
          this.toastr.success(toastrMessage.message, toastrMessage.title, { disableTimeOut: true }).onHidden.subscribe(notification => {
            this.notificationQueue.pop();
          });
          break;
        }
        case ('error'): {
          this.toastr.error(toastrMessage.message, toastrMessage.title, { disableTimeOut: false }).onHidden.subscribe(notification => {
            this.notificationQueue.pop();
          });
          break;
        }
        case ('warning'): {
          this.toastr.warning(toastrMessage.message, toastrMessage.title, { disableTimeOut: false }).onHidden.subscribe(notification => {
            this.notificationQueue.pop();
          });
          break;
        }
        case ('info'): {
          this.toastr.info(toastrMessage.message, toastrMessage.title, { disableTimeOut: false }).onHidden.subscribe(notification => {
            this.notificationQueue.pop();
          });
          break;
        }
      }
    } else {
      const toastrQueue = this.notificationQueue;
      // this.toastrQueue = this.notificationQueue;
      this.toastr.clear();
      this.showCollection(toastrQueue);
    }
  }
  showToastr(toastrQueue, disableTimeOut: boolean = false) {
    toastrQueue.forEach((item) => {
      switch (item.messageType) {
        case ('success'): {
          this.toastr.success(item.message, item.title, { disableTimeOut: disableTimeOut });
          break;
        }
        case ('error'): {
          this.toastr.error(item.message, item.title, { disableTimeOut: disableTimeOut });
          break;
        }
        case ('warning'): {
          this.toastr.warning(item.message, item.title, { disableTimeOut: disableTimeOut });
          break;
        }
        case ('info'): {
          this.toastr.info(item.message, item.title, { disableTimeOut: disableTimeOut });
          break;
        }
      }
    });
  }

  showValidationMsg(message, title: string = 'Validation', messageType = 'warning') {
    const toastrMessage = new ToastrMessage;
    toastrMessage.message = message;
    toastrMessage.title = title;
    toastrMessage.messageType = messageType;

    const duplicateToaster = this.toastr.findDuplicate(toastrMessage.title,toastrMessage.message, false, false);
    if (duplicateToaster == null) {
      this.validationMsgQueue.push(toastrMessage);
      this.showValidationNotifications(toastrMessage);
    }
  }

  showValidationNotifications(toastrMessage) {
    if (this.validationMsgQueue.length > 0 && this.validationMsgQueue.length <= 3) {
      this.toastr.warning(toastrMessage.message, toastrMessage.title, { disableTimeOut: false }).onTap.subscribe(notification => {
        this.notificationQueue.pop();
      });
    } else {
      const toastrQueue = this.validationMsgQueue;
      // this.toastrQueue = this.notificationQueue;
      this.toastr.clear();
      this.showValidationCollection(toastrQueue);
    }
  }

  showValidationCollection(toastrQueue) {
    const collectionToastr = this.toastr.info('Click to view notifications',
      'Show More', { disableTimeOut: true });
    collectionToastr.onShown.subscribe(notification => {
      this.toastr.toastrConfig.disableTimeOut = true;
    });
    collectionToastr.onTap.subscribe(notification => {
      this.toastr.toastrConfig.maxOpened = 0;
      this.showToastr(toastrQueue, false);
      this.validationMsgQueue = [];
    });
  }
}
