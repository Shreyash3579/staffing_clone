import {AfterViewChecked, Component, ElementRef, Input, OnChanges, SimpleChanges, ViewChild} from "@angular/core";
import {animate, state, style, transition, trigger} from "@angular/animations";

@Component({
  selector: "app-staffing-plan",
  standalone: false,
  templateUrl: "./staffing-plan.component.html",
  styleUrl: "./staffing-plan.component.scss",
  animations: [trigger("fadeInSlow", [state("hidden", style({opacity: 0})), state("visible", style({opacity: 1})), transition("hidden => visible", [animate("1000ms ease-in") // Adjust duration as needed
  ]), ]), trigger("expandEdit", [
    transition(":enter", [
      style({width: "733px", opacity: 0}),
      animate("300ms ease-in", style({width: "700px", opacity: 1}))]),
    transition(":leave", [animate("300ms ease-out", style({
      width: "733px",
      opacity: 0
    }))])]), trigger("expandEdit2", [transition(":enter", [style({
    width: "{{startWidth}}px",
    opacity: 0
  }), animate("300ms ease-in", style({width: "{{endWidth}}px", opacity: 1}))], {
    params: {
      startWidth: 720,
      endWidth: 700
    }
  }), transition(":leave", [animate("300ms ease-out", style({width: "{{startWidth}}px", opacity: 0}))], {params: {startWidth: 720}})])]
})
export class StaffingPlanComponent implements OnChanges {
  @Input() chatExample;
  timeLine = require("../mocked/timelineData.json");
  resourceAllocation = require("../mocked/resourceAllocations.json");
  resources = require("../mocked/resources.json");
  hoveredDiv: any = null;
  mouseX: number = 0;
  mouseY: number = 0;
  idTooltip: number | null = null;
  editableIndex: number | null = null;
  newMessage = "";
  scrollable = false;
  currentLength = 0;
  visibility = "hidden";
  startWidth = 0;
  firstAfterViewChecked = true;
  protected readonly console = console;
  @ViewChild("scrollContainer") private scrollContainer!: ElementRef;

  ngOnInit() {
    setTimeout(() => {
      this.visibility = "visible";
    }, 100);
  }

  ngOnChanges(changes: SimpleChanges) {
    if (changes["chatExample"]) {
      console.log("chatExample changed:", changes["chatExample"].currentValue);
      this.currentLength = changes["chatExample"].currentValue.length;
    }
  }

  onMouseMove(event: MouseEvent) {
    console.log(event);
    this.mouseX = event.clientX;
    this.mouseY = event.clientY;
  }

  setEditable(index: number, parentEl: string) {
    this.editableIndex = index;
    this.newMessage = this.chatExample[index].message[this.chatExample[index].message.length - 1];
    this.startWidth = document.getElementById(parentEl).offsetWidth - 20;
  }

  cancelEdit() {
    this.editableIndex = null;
    this.startWidth = 0;
  }

  sendMessage(i: number) {
    this.chatExample[i].message.push(this.newMessage);
    this.newMessage = "";
    this.editableIndex = null;
    this.chatExample[i + 1].loading = true;
    this.chatExample[i + 2].loading = true;
    setTimeout(() => {
      this.chatExample[i + 1].loading = false;
      this.chatExample[i + 2].loading = false;
    }, 4000);
  }

  ngAfterViewChecked() {
    if (this.currentLength < this.chatExample.length || (this.firstAfterViewChecked && this.currentLength > 0)) {
      this.scrollToBottom();
      this.firstAfterViewChecked = false;
      this.currentLength = this.chatExample.length;
    }
  }

  scrollToBottom(): void {
    const element = this.scrollContainer.nativeElement;
    const bottomElement = element.lastElementChild;

    if (bottomElement) {
      bottomElement.scrollIntoView({behavior: "smooth", block: "end"});
    }
  }

  onShowChangesClick(i) {
    this.chatExample[i].resources.showChanges = true;
  }

  onHideChangesClick(i) {
    this.chatExample[i].resources.showChanges = false;
  }

  getDeletedRowIndex(i): number {
    return this.chatExample[i].resources.resources.findIndex(resource => resource.deleted);
  }

  shouldApplySlideIn(index: number, i): boolean {
    const deletedRowIndex = this.getDeletedRowIndex(i);
    return index < deletedRowIndex && this.chatExample[i].resources.showChanges;
  }
}
