import { Component, OnInit, Output, EventEmitter } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { ValidationService } from 'src/app/shared/validationService';

@Component({
  selector: 'app-join-playground-pop-up',
  templateUrl: './join-playground-pop-up.component.html',
  styleUrls: ['./join-playground-pop-up.component.scss']
})
export class JoinPlaygroundPopUpComponent implements OnInit {
  @Output() joinPlaygroundEmitter = new EventEmitter();
  
  playgroundIdToJoin = "";
  playgroundValidationObj = {
    isInValid : false,
    errorMessage: ''
  }

  constructor(public bsModalRef: BsModalRef) { }

  ngOnInit(): void {
  }

  onJoinPlaygroundClick(){
    if(this.isValidPlaygroundId()){
        this.joinPlaygroundEmitter.emit(this.playgroundIdToJoin);
        this.closeDialogHandler();
    }else{
      return;
    }
  }

  isValidPlaygroundId(){
    this.playgroundValidationObj.isInValid = false;
    this.playgroundValidationObj.errorMessage = "";
    
    if(!ValidationService.isValidGUID(this.playgroundIdToJoin)){
      this.playgroundValidationObj.isInValid = true;
      this.playgroundValidationObj.errorMessage = "Please enter a valid Id to join a Whiteboard session!";
      return false;
    }

    return true;
  }

  closeDialogHandler() {
    this.bsModalRef.hide();
  }

}
