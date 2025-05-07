import { Component, EventEmitter, OnDestroy, OnInit, Output, SimpleChanges } from '@angular/core';
import { select, Store } from '@ngrx/store';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { Subscription } from 'rxjs';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import { CommitmentType } from 'src/app/shared/interfaces/commitmentType.interface';
import { PracticeRingfencesDetails } from 'src/app/shared/interfaces/practice-ringfences-detail';
import { PracticeArea } from 'src/app/shared/interfaces/practiceArea.interface';
import { LocalStorageService } from 'src/app/shared/local-storage.service';

// --------------------------Redux Component -----------------------------------------//
import * as adminActions from '../State/admin.actions';
import * as fromAdmin from '../State/admin.selectors';
import * as adminState from '../State/admin.state';

@Component({
  selector: 'app-practice-ringfences',
  templateUrl: './practice-ringfences.component.html',
  styleUrls: ['./practice-ringfences.component.scss']
})
export class PracticeRingfencesComponent implements OnInit, OnDestroy {

  practiceBasedRingfences: CommitmentType[] =[];
  practiceAreaDropDownList;
  practiceAreas: PracticeArea[] = [];
  
  practiceRingfencesData: PracticeRingfencesDetails[]=[];
  selectedPracticeBaseRinfence : PracticeRingfencesDetails = {} as PracticeRingfencesDetails;
 
  //-----------------Store Subscriptions------------------------------//
  public storeSub: Subscription = new Subscription();
  
  @Output() upsertPracticeBasedRingfenceEmitter = new EventEmitter<any>();
  
  constructor(public bsModalRef: BsModalRef,  
    private localStorageService: LocalStorageService,
    private store: Store<adminState.State>,
  ) { }

  ngOnInit(): void {
    this.getLookUpdataFromLocalStorage();
    this.setStoreSubscription();
    this.getPracticeBasedRingfenceDetails();       
  }

  private setStoreSubscription() {
    this.storeSub.add(this.store.pipe(
      select(fromAdmin.getPracticeBasedRingfences)
    ).subscribe((practiceBasedRingfences: CommitmentType[]) => {
      this.practiceBasedRingfences = practiceBasedRingfences;
      this.intilializeDropDowns();
    }));
  }

  private getPracticeBasedRingfenceDetails() {
    const showHidden  = true;
    this.store.dispatch(
      new adminActions.LoadPracticeBasedRingfences(showHidden)
    );
  }

  getLookUpdataFromLocalStorage() {
    this.practiceAreas = this.localStorageService.get(ConstantsMaster.localStorageKeys.practiceAreas);
  }
 
  intilializeDropDowns(){
    this.practiceRingfencesData = [];
    this.practiceAreas.forEach((practiceArea) => {
      const savedPracticeBasedRingfence = this.practiceBasedRingfences.find((x) => x.commitmentTypeCode === practiceArea.practiceAreaCode.toString());
      
      const practiceRingfencesDetails: PracticeRingfencesDetails = {
        practiceAreaName: practiceArea.practiceAreaName,
        practiceAreaCode: practiceArea.practiceAreaCode, // maps to commitment Type code on saving
        practiceAreaAbbreviation: practiceArea.practiceAreaAbbreviation,
        commitmentTypeName: savedPracticeBasedRingfence?.commitmentTypeName ?? "",
        allowsStaffingInAmericas: savedPracticeBasedRingfence?.allowsStaffingInAmericas,
        allowsStaffingInEMEA: savedPracticeBasedRingfence?.allowsStaffingInEMEA,
        allowsStaffingInAPAC: savedPracticeBasedRingfence?.allowsStaffingInAPAC
      };

      this.practiceRingfencesData.push(practiceRingfencesDetails);
    });

    this.practiceAreaDropDownList = this.practiceRingfencesData.map(x => {
       return {
          text: x.practiceAreaName,
          value: x.practiceAreaCode
        }
    });

  }
 
  setSelectedPracticeAreaList(practiceAreaCodes) {
    this.selectedPracticeBaseRinfence = this.practiceRingfencesData.find((x) => x.practiceAreaCode.toString() === practiceAreaCodes.value);
  }

 
  onSaveClick() {
    const practiceRingfenecToUpsert : CommitmentType = {
      commitmentTypeCode: this.selectedPracticeBaseRinfence.practiceAreaCode.toString(),
      commitmentTypeName: `${this.selectedPracticeBaseRinfence.practiceAreaAbbreviation} (${this.selectedPracticeBaseRinfence.practiceAreaName})`,
      allowsStaffingInAmericas: !!this.selectedPracticeBaseRinfence.allowsStaffingInAmericas,
      allowsStaffingInEMEA: !!this.selectedPracticeBaseRinfence.allowsStaffingInEMEA,
      allowsStaffingInAPAC: !!this.selectedPracticeBaseRinfence.allowsStaffingInAPAC,
      precedence: 0,
      reportingPrecedence: 0,
      isStaffingTag: true
     }; 

    this.upsertPracticeBasedRingfenceEmitter.emit(practiceRingfenecToUpsert);
    this.closeForm();
  }

  closeForm() {
    this.bsModalRef.hide();
  }

  ngOnDestroy() {
    this.storeSub.unsubscribe();
  }

}
