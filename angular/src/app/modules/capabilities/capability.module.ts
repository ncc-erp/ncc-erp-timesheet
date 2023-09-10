import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { AngularEditorModule } from "@kolkov/angular-editor";
import { SharedModule } from "@shared/shared.module";
import { NgxMatSelectSearchModule } from "ngx-mat-select-search";
import { NgxPaginationModule } from "ngx-pagination";
import { CapabilityRoutingModule } from "./capability-routing.module";
import { CapabilityComponent } from "./capability.component";
import { CreateEditCapabilityDialogComponent } from "./create-edit-capability-dialog/create-edit-capability-dialog.component";
@NgModule({
    declarations: [CapabilityComponent, CreateEditCapabilityDialogComponent],
    imports: [
        CommonModule,
        SharedModule,
        FormsModule,
        ReactiveFormsModule,
        CapabilityRoutingModule,
        NgxMatSelectSearchModule,
        AngularEditorModule,
        NgxPaginationModule
    ],
    entryComponents: [CreateEditCapabilityDialogComponent],
    exports:[CreateEditCapabilityDialogComponent]
})
export class CapabilityModule{
}