import { Component, Input, Output, EventEmitter } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { ConstantsMaster } from 'src/app/shared/constants/constantsMaster';
import { ResourceViewSelectedTab } from 'src/app/shared/interfaces/resource-view-selected-tab.interface';
import { LocalStorageService } from 'src/app/shared/local-storage.service';


@Component({
  selector: 'resources-resource-info-tabs',
  templateUrl: './resource-info-tabs.component.html',
  styleUrls: ['./resource-info-tabs.component.scss']
})
export class ResourceInfoTabsComponent {
  @Input() resourceViewNotes: any;
  @Input() resourceViewCD: any;
  @Input() resourceViewCommercialModel: any;
  @Input() rowIndex: number;
  @Input() isNotesReadonly: boolean;
  @Input() employeeCode: string;
  @Input() isCollapsed: boolean;

  @Output() upsertNote = new EventEmitter<any>();
  @Output() deleteNotes = new EventEmitter<any>();
  @Output() upsertRecentCD = new EventEmitter<any>();
  @Output() deleteRecentCD = new EventEmitter<any>();
  @Output() upsertCommercialModel = new EventEmitter<any>();
  @Output() deleteCommercialModel = new EventEmitter<any>();
  @Output() selectedResourceViewTab = new EventEmitter<any>();

  selectedTab = 'Notes';
  isPdfExport = false;
  hideAddNewNoteAndCD= false;
  public notesAlertText = ConstantsMaster.NotesAlert;
  floatElement:boolean = false;
  isNotesCdAndCommercialModelWrapperCollapsed = false;
  recentCdInfo = ConstantsMaster.RecentCd;
  commercialModelInfo = ConstantsMaster.CommercialModel;
  constructor(private activatedRoute: ActivatedRoute, private localStorageService: LocalStorageService){

  }

  ngOnInit() {
    this.checkPdfExport();
    this.loadSelectedTabForEmployee();
  }
  
  private checkPdfExport(): void {
    this.activatedRoute.queryParams.subscribe(params => {
      this.isPdfExport = params['export'];
    });
  
    // If the export is true, hide the "Add New Note and CD" section
    if (this.isPdfExport) {
      this.hideAddNewNoteAndCD = true;
    }
  }
  
  private loadSelectedTabForEmployee(): void {
    if (this.isPdfExport) {
      const resourceViewSelectedTabs = this.getResourceViewSelectedTabsFromLocalStorage();
  
      if (resourceViewSelectedTabs && resourceViewSelectedTabs.length > 0) {
        this.setSelectedTabForResource(resourceViewSelectedTabs);
      }
    }
  }
  
  private getResourceViewSelectedTabsFromLocalStorage(): ResourceViewSelectedTab[] {
    return this.localStorageService.get(ConstantsMaster.localStorageKeys.resourceViewSelectedTabs);
  }
  
  private setSelectedTabForResource(resourceViewSelectedTabs: ResourceViewSelectedTab[]): void {
    const selectedTabForResource= resourceViewSelectedTabs.find(tab => tab.employeeCode === this.employeeCode);
  
    if (selectedTabForResource) {
      this.selectedTab = selectedTabForResource.selectedTab;
    } else {
      this.selectedTab = 'Notes';
    }
  }

  selectTab(selectedTab: string): void {
    this.selectedTab = selectedTab;
   this.selectedResourceViewTab.emit(selectedTab);
  }

  getCommaSeparatedNotes(resourceViewNotes:any): string {
    return resourceViewNotes.map(note => note.note).filter(note => note).join(', ');
  }

  getCommaSeparatedRecentCDs(resourceViewCD:any): string {
    return resourceViewCD.map(cd => cd.recentCD).filter(cd => cd).join(', ');
  }

  getCommaSeparatedCommercialModels(resourceViewCommercialModel:any): string {
    return resourceViewCommercialModel.map(cm => cm.commercialModel).filter(cm => cm).join(', ');
  }

  upsertResourceViewNoteHandler(event: any) {
    this.upsertNote.emit(event);
  }

  deleteResourceViewNotesHandler(event: any) {
    this.deleteNotes.emit(event);
  }

  upsertResourceViewRecentCDHandler(event: any) {
    this.upsertRecentCD.emit(event);
  }

  deleteResourceViewRecentCDHandler(event: any) {
    this.deleteRecentCD.emit(event);
  }

  upsertResourceViewCommercialModelHandler(event: any) {
    this.upsertCommercialModel.emit(event);
  }

  deleteResourceViewCommercialModelHandler(event: any) {
    this.deleteCommercialModel.emit(event);
  }

  closeNotesOrRecentCDWrapperEmitterHandler(event: any) {
    if(event){
      this.floatElement = false;
      this.isNotesCdAndCommercialModelWrapperCollapsed = true;
    }

  }

  onClickRecentCD(): void {
    if(!this.isNotesCdAndCommercialModelWrapperCollapsed){
      this.floatElement = true;
      this.selectedTab = 'Recent CD';
    }
    else{
      this.isNotesCdAndCommercialModelWrapperCollapsed = false;
    }
  }

  onClickCommercialModel(): void {
    if(!this.isNotesCdAndCommercialModelWrapperCollapsed){
      this.floatElement = true;
      this.selectedTab = 'Commercial Model';
    }
    else{
      this.isNotesCdAndCommercialModelWrapperCollapsed = false;
    }
  }
  onClickNotes(): void {
    if(!this.isNotesCdAndCommercialModelWrapperCollapsed){
      this.floatElement = true;
      this.selectedTab = 'Notes';
    }
    else{
      this.isNotesCdAndCommercialModelWrapperCollapsed = false;
    }
    
  }

}