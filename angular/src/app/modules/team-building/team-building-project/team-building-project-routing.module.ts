import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { TeamBuildingProjectComponent } from "./team-building-project.component";

const routes: Routes = [
    {path: '', component: TeamBuildingProjectComponent}
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class TeamBuildingProjectRoutingModule {}
