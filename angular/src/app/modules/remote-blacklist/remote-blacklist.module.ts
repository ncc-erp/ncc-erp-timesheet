import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { SharedModule } from "@shared/shared.module";
import { RemoteBlacklistRoutingModule } from "./remote-blacklist-routing.module";
import { RemoteBlacklistComponent } from "./remote-blacklist.component";
import { AddUserToRemoteBlacklistDialogComponent } from "./add-user-to-remote-blacklist-dialog/add-user-to-remote-blacklist-dialog.component";
import { ImportCsvDialogComponent } from "./import-csv-dialog/import-csv-dialog.component";
import { NgxMatSelectSearchModule } from 'ngx-mat-select-search';
import { NgxPaginationModule } from "ngx-pagination";
import { MatProgressSpinnerModule } from "@angular/material/progress-spinner";

@NgModule({
    declarations: [RemoteBlacklistComponent, AddUserToRemoteBlacklistDialogComponent, ImportCsvDialogComponent],
    imports: [
        CommonModule,
        FormsModule,
        SharedModule,
        RemoteBlacklistRoutingModule,
        NgxMatSelectSearchModule,
        NgxPaginationModule,
        MatProgressSpinnerModule
    ],
    entryComponents: [AddUserToRemoteBlacklistDialogComponent, ImportCsvDialogComponent]
})
export class RemoteBlacklistModule{

}