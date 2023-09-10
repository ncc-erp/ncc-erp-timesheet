import {NgModule} from '@angular/core';
import {Routes, RouterModule} from '@angular/router';
import {ApproveAbsenceRequestComponent} from "@app/modules/approve-absence-request/approve-absence-request.component";

const routes: Routes = [{
    path: '', component: ApproveAbsenceRequestComponent
}];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class ApproveAbsenceRequestRoutingModule {
}
