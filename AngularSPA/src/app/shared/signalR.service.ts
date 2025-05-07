import { Injectable } from '@angular/core';
import * as signalR from "@microsoft/signalr"
import { HttpTransportType, HubConnection, HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
import { Observable } from 'rxjs';
import { Subject } from 'rxjs/internal/Subject';
import { PegOpportunity } from './interfaces/pegOpportunity.interface';
import { NotesAlertDialogService } from '../overlay/dialogHelperService/notesAlertDialog.service';

@Injectable({
  providedIn: 'root'
})
export class SignalrService {

  public hubConnection: HubConnection;
  public pegUpdatedDataSubject$: Subject<any> = new Subject<any>();
  public caseIntakeBannerSubject$: Subject<any> = new Subject<any>();
  public caseIntakeBanner$ = this.caseIntakeBannerSubject$.asObservable();


  constructor(private notesAlertDialogService: NotesAlertDialogService) {  
  }

  public createConnection(environmentUrl: string, employeecode: string) {
    
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${environmentUrl}/signalRHub/offers?employeecode=${employeecode}`)
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information)
      .build();
  }

  public startConnection() {
    if (this.hubConnection.state === HubConnectionState.Connected) {
      return;
    }

    this.hubConnection.start().then(
      () => {
        console.log('Hub connection started!@@S');
      },
      error => console.error(error)
    );

    this.hubConnection.onreconnecting((error) => {
      console.log('Core Hub Reconnecting');
    });

    this.hubConnection.onreconnected((error) => {
      console.log('Core Hub Reconnected');
    });
  }

  public registerOnServerEvents(): void {
    this.hubConnection.on('SendPegOpportunityUpdates', (object: PegOpportunity) => {
      console.log("the offer is received");
      this.pegUpdatedDataSubject$.next(object); 
    });

    this.hubConnection.on('SendUnreadSharedNotesUpdate', () => {
      console.log("the offer is received");
      this.notesAlertDialogService.showBanner.next(true);
      
    });

    this.hubConnection.on('SendUnreadCaseIntakeUpdate', () => {
      console.log("the case Intake update is received");
      this.caseIntakeBannerSubject$.next(true);
      
    });

  }

  public onPegDataUpdated(): Observable<any> {
    return this.pegUpdatedDataSubject$.asObservable();
  }

}
