import {Component, EventEmitter, Input, OnInit, Output} from "@angular/core";
import {SharedService} from "../../shared/shared.service";
import {Project} from "../../shared/interfaces/project.interface";
import {NgForOf} from "@angular/common";

@Component({
  selector: 'app-sidebar-project-list',
  standalone: true,
  imports: [
    NgForOf
  ],
  templateUrl: './sidebar-project-list.component.html',
  styleUrl: './sidebar-project-list.component.scss'
})
export class SidebarProjectListComponent{
  checkedProjects: string[] = [];
  @Input() projects: any[] = [];
  // Accept the selected projects from the parent JSON
  @Input() selectedProjects: any[] = [];

  // Emit an event when selected projects are updated in this component
  @Output() selectedProjectsChange = new EventEmitter<any[]>();

  // Example method that might be called when selection changes
  onProjectSelectionChange(): void {
    this.selectedProjectsChange.emit(this.checkedProjects);
  }
  toggleProjectCheck(projectId: string) {
    const index = this.checkedProjects.indexOf(projectId);
    if (index === -1) {
      this.checkedProjects.push(projectId);
    } else {
      this.checkedProjects.splice(index, 1);
    }
    this.onProjectSelectionChange();
  }
  isProjectChecked(projectId: string): boolean {
    return this.checkedProjects.includes(projectId);
  }
}
