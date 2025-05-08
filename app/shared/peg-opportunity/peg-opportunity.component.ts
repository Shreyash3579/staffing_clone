import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { DomSanitizer, SafeResourceUrl } from '@angular/platform-browser';
import { BsModalRef } from 'ngx-bootstrap/modal';

// ----------------------- Component/Service References ----------------------------------//
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-peg-opportunity',
  templateUrl: './peg-opportunity.component.html',
  styleUrls: ['./peg-opportunity.component.scss']
})
export class PegOpportunityComponent implements OnInit {

  // -----------------------Input Params from Dialog Initial State--------------------------------------------//
  pegOpportunityId: string;

  // -----------------------Constructor--------------------------------------------//
  constructor(private sanitizer : DomSanitizer, private bsModalRef: BsModalRef) { }

  @ViewChild('loginIframe', { static: true })
  private Iframe: ElementRef<HTMLIFrameElement>;
  url: string = `${environment.settings.pegC2COpportunityDetailsUrl}`;;
  urlSafe: SafeResourceUrl;
  iFrameUrl: SafeResourceUrl;
  displayIFrame = true;

  onOpenIFrame(): void{
    // var iframe = document.createElement('iframe');
    //     iframe.setAttribute("src", "https://www.youtube.com/embed/test");
    //     iframe.setAttribute("width", "100%");
    //     iframe.setAttribute("height", "315");
    //     iframe.setAttribute("frameborder", "0");
    //     iframe.setAttribute("allowfullscreen", "1");
    //     document.getElementById("wrapper_video").appendChild(iframe);
    this.iFrameUrl=this.url;
    //var iFrame = document.getElementById("loginIframe");
    var WindowPostMessageOptions={targetOrigin:''};
    WindowPostMessageOptions.targetOrigin="*";
    const message = {message:"Hello from iframe"};
    //console.log(this.Iframe);
    this.Iframe.nativeElement.innerText='text';
    //-----------------------FINAL QUERY STRING APPROACH-------------
    var _iframe=document.getElementById('pegIframe')as HTMLImageElement
    var resultset= JSON.stringify({employeeCode: '53142' , opportunityid: 'R21061'})
    //var encrypted = CryptoJS.AES.encrypt("Message", "Secret Passphrase");
    _iframe.src=_iframe.src + resultset;
  //this.Iframe.nativeElement.contentWindow.postMessage(message, this.url);
    document.getElementById('pegIframe').style.display='block';
    //-----------------------FINAL QUERY STRING APPROACH END-------------
  }

  // -----------------------Life Cycle Events--------------------------------------------//
 
  ngOnInit(): void {
    const encodedURI = `${this.url}${encodeURIComponent(this.pegOpportunityId)}`;
    this.urlSafe = this.sanitizer.bypassSecurityTrustResourceUrl(encodedURI);
    this.onOpenIFrame();
  }

  // -----------------------Event Handlers--------------------------------------------//

  closeForm(){
    this.bsModalRef.hide();
  }
}
