import { NgModule } from "@angular/core";
import { Routes, RouterModule } from "@angular/router";
import { TeamBuildingRequestComponent } from "./team-building-request.component";

const routes: Routes = [
    {path: '', component: TeamBuildingRequestComponent}
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class TeamBuildingRequestRoutingModule {}
