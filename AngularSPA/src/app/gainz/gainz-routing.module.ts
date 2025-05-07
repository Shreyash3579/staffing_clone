import { NgModule } from "@angular/core";
import { Routes, RouterModule } from "@angular/router";
import {ChatAssistantComponent} from "./chat-assistant/chat-assistant.component";

const routes: Routes = [
  {
    path: "",
    component: ChatAssistantComponent
  },
  {
    path: "**",
    component: ChatAssistantComponent // or another component that should handle unknown sub-paths
  }
];
@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class GainzRoutingModule { }
