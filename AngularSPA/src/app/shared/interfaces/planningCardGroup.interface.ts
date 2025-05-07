import { PlanningCard } from './planningCard.interface';
import { Project } from './project.interface';

export interface PlanningCardGroup {
  groupTitle: string;
  planningCards: PlanningCard[];
}
