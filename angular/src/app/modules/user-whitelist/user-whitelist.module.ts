import { NgModule } from "@angular/core";
import { UserWhitelistComponent } from "./user-whitelist.component";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { SharedModule } from "@shared/shared.module";
import { UserWhitelistRoutingModule } from "./user-whitelist-routing.module";
import { NgxPaginationModule } from "ngx-pagination";
import { AddEditUserWhitelistComponent } from "./add-edit-user-whitelist/add-edit-user-whitelist.component";
import { NgxMatSelectSearchModule } from "ngx-mat-select-search";
import { ImportExcelComponent } from "./import-excel/import-excel.component";

@NgModule({
    declarations: [UserWhitelistComponent, AddEditUserWhitelistComponent, ImportExcelComponent],
    imports: [
        CommonModule,
        FormsModule,
        SharedModule,
        UserWhitelistRoutingModule,
        NgxPaginationModule,
        NgxMatSelectSearchModule
    ],
    entryComponents: [AddEditUserWhitelistComponent, ImportExcelComponent]
})
export class UserWhitelistModule { 

}