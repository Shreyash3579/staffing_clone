// ----------------------- Angular Package References ----------------------------------//
import { Component, OnInit, Input, OnDestroy } from '@angular/core';
import { skip } from 'rxjs/operators';

// ----------------------- Component/Service References ----------------------------------//
import { environment } from '../../../../environments/environment';
import { LocalStorageService } from '../../../shared/local-storage.service';
import { NotificationService } from 'src/app/shared/notification.service';
import { TableauService } from '../../common/tableau.service';


@Component({
  selector: 'analytics-share-url',
  templateUrl: './share-url.component.html'
})
export class ShareUrlComponent implements OnInit, OnDestroy {

  // -----------------------Local Variables--------------------------------------------//
  taburl: string = '';
  hrefurl: string = '';
  bshowUrlValue: boolean;

  // -----------------------Input Variables--------------------------------------------//
  @Input() url: string;
  @Input() envPageName: string;
  @Input() serviceInstance: any;
  // -------------------Constructor----------------------//
  constructor(
    private localStorageService: LocalStorageService,
    private notificationService: NotificationService,
  ) { }

  // -------------------Component LifeCycle Events and Functions----------------------//
  ngOnInit() {
    if (!this.serviceInstance) {
      this.serviceInstance = new TableauService(this.url, this.localStorageService, `${environment.settings.environmentUrl}`);
    }
  }
  ngOnDestroy() {
    // this.serviceInstance.getmyTaburl.unsubscribe();
    // this.serviceInstance.mytaburl.next(this.taburl);
  }
  showcopymessage() {
    this.notificationService.showInfo('Url Value Copied to Clipboard!!');
  }
  // TableauURL
  public generateUrl() {
    this.serviceInstance.callShare(this.envPageName);
    var bfirstload = true;
    this.serviceInstance.getmyTaburl.pipe(skip(1)).subscribe((data) => {
      this.taburl = data;
      this.hrefurl = `mailto:${environment.settings.globalStaffingSupportMailbox}?subject=Staffing Tableau Custom Shared URL&body=`+ this.taburl;
      var re = /original_view=yes/gi
      if (this.taburl.search(re) != -1) {
        if (bfirstload) {
          this.bshowUrlValue = false;
          this.taburl = "";
          this.notificationService.showWarning('URL for Original View cannot be Generated!!');
          bfirstload = false;
        }
      }
      else {
        if (bfirstload) {
          this.notificationService.showSuccess('Url Generated for Customer View!!');
          this.bshowUrlValue = true;
          bfirstload = false;
        }
      }
    });
  }

}
