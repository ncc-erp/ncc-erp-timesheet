import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { AngularEditorModule } from "@kolkov/angular-editor";
import { SharedModule } from "@shared/shared.module";
import { NgxMatSelectSearchModule } from "ngx-mat-select-search";
import { NgxPaginationModule } from "ngx-pagination";
import { CapabilitySettingRoutingModule } from "./capability-setting-routing.module";
import { CapabilitySettingComponent } from "./capability-setting.component";
import { CloneCapabilitySettingDialogComponent } from "./create-edit-capability-setting/clone-capability-setting-dialog/clone-capability-setting-dialog.component";
import { CreateEditCapabilitySettingComponent } from "./create-edit-capability-setting/create-edit-capability-setting.component";
import { UpdateCapabilitySettingComponent } from './create-edit-capability-setting/update-capability-setting/update-capability-setting.component';
@NgModule({
    declarations: [CapabilitySettingComponent, CreateEditCapabilitySettingComponent,CloneCapabilitySettingDialogComponent, UpdateCapabilitySettingComponent],
    imports: [
        CommonModule,
        SharedModule,
        FormsModule,
        ReactiveFormsModule,
        CapabilitySettingRoutingModule,
        NgxMatSelectSearchModule,
        AngularEditorModule,
        NgxPaginationModule
    ],
    entryComponents: [CreateEditCapabilitySettingComponent, CloneCapabilitySettingDialogComponent, UpdateCapabilitySettingComponent],
    exports: [CreateEditCapabilitySettingComponent, CloneCapabilitySettingDialogComponent]
})
export class CapabilitySettingModule {
}