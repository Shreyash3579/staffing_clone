import { PlanningCard } from './planningCard.interface';
import { Project } from './project.interface';

export interface ProjectsGroup {
  groupTitle: string;
  projects: Project[];
  planningCards: PlanningCard[];
}
