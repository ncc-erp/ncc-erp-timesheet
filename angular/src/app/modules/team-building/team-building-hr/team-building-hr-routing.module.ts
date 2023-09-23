import { NgModule } from "@angular/core";
import { RouterModule, Routes } from "@angular/router";
import { TeamBuildingHrComponent } from "./team-building-hr.component";

const routes: Routes = [
    {path: '', component: TeamBuildingHrComponent}
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class TeamBuildingHRRoutingModule {}
