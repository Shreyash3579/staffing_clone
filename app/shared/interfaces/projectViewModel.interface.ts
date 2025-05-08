import { Project } from "./project.interface";

export interface ProjectViewModel {
    projects: Project[];
    pinnedProjects: Project[];
    hiddenProjects: Project[];
  }