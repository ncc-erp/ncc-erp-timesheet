import { NgModule } from "@node_modules/@angular/core";
import { TrackerWhitelistComponent } from "./tracker-whitelist.component";
import { CommonModule } from "@node_modules/@angular/common";
import { FormsModule } from "@node_modules/@angular/forms";
import { SharedModule } from "@shared/shared.module";
import { TrackerWhitelistRoutingModule } from "./tracker-whitelist-routing.module";
import { NgxPaginationModule } from "ngx-pagination";

@NgModule({
    declarations: [TrackerWhitelistComponent],
    imports: [
        CommonModule,
        FormsModule,
        SharedModule,
        TrackerWhitelistRoutingModule,
        NgxPaginationModule
    ],
    entryComponents: []
})
export class TrackerWhitelistModule { 

}