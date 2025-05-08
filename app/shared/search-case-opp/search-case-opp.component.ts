import { Component, EventEmitter, OnInit, Output, ViewEncapsulation } from '@angular/core';
import { BsModalRef } from 'ngx-bootstrap/modal';
import { Observable } from 'rxjs';
import { debounceTime, mergeMap } from 'rxjs/operators';
import { HomeService } from 'src/app/home/home.service';
import { ConstantsMaster } from '../constants/constantsMaster';
import { Project } from '../interfaces/project.interface';
import { PopupDragService } from '../services/popupDrag.service';
import { ValidationService } from '../validationService';

@Component({
    selector: 'app-search-case-opp',
    templateUrl: './search-case-opp.component.html',
    styleUrls: ['./search-case-opp.component.scss']
})
export class SearchCaseOppComponent implements OnInit {

    errorMessage = '';
    asyncProjectString = '';
    projects: Observable<Project[]>;
    selectedCase;
    
    noCaseOppFound = false;
    public noResultsFoundMsg = ValidationService.noResultsFoundMsg;
    public planningCardActions = ConstantsMaster.PlanningCardMergeActions;
    @Output() addCaseOppEmitter = new EventEmitter<any>();

    //----------------------------Input/Initial State Data --------------------------------//
    public modalHeaderText: string;
    public showMergeAndCopy?: boolean;
    public searchCases?: boolean;
    public searchOpps?: boolean;

    constructor(public bsModalRef: BsModalRef,
        private homeService: HomeService,
        private _popupDragService: PopupDragService) { 
    }

    ngOnInit() {
        this.showMergeAndCopy = this.showMergeAndCopy ?? true;
        this.searchCases = this.searchCases ?? true;
        this.searchOpps = this.searchOpps ?? true;
        //this._popupDragService.dragEvents();
    }

    onSearchItemSelectHandler(selectedCaseOpp){
        this.selectedCase = selectedCaseOpp;
    }

    onAddCaseOppClick(action) {
        if(this.selectedCase) {
            this.addCaseOppEmitter.emit({selectedCase: this.selectedCase, action: action});

            this.closeDialogHandler();
        } else {
            this.errorMessage = 'Please select a Case/Opp from the list';
        }
    }

    closeDialogHandler() {
        this.errorMessage = '';
        this.bsModalRef.hide();
    }

}