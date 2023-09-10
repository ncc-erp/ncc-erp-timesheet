import { NgModule } from "@angular/core";
import { Routes, RouterModule } from "@angular/router";
import { TeamBuildingPmComponent } from "./team-building-pm.component";

const routes: Routes = [
    {path: '', component: TeamBuildingPmComponent}
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class TeamBuildingPMRoutingModule {}
