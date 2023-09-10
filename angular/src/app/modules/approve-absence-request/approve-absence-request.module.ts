import {NgModule} from '@angular/core';
import {CommonModule} from '@angular/common';

import {ApproveAbsenceRequestRoutingModule} from './approve-absence-request-routing.module';
import {ApproveAbsenceRequestComponent} from "@app/modules/approve-absence-request/approve-absence-request.component";
import {SharedModule} from "@shared/shared.module";
import {FormsModule} from "@node_modules/@angular/forms";
@NgModule({
    declarations: [ApproveAbsenceRequestComponent],
    imports: [
        CommonModule,
        ApproveAbsenceRequestRoutingModule,
        SharedModule,
        FormsModule,
    ],
    entryComponents: []
})
export class ApproveAbsenceRequestModule {
}
