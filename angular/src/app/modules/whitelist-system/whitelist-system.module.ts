import { NgModule } from "@angular/core";
import { WhitelistSystemComponent } from "./whitelist-system.component";
import { CommonModule } from "@angular/common";
import { FormsModule } from "@angular/forms";
import { SharedModule } from "@shared/shared.module";
import { WhitelistSystemRoutingModule } from "./whitelist-system-routing.module";
import { NgxPaginationModule } from "ngx-pagination";
import { CreateEditWhitelistSystemComponent } from "./create-edit-whitelist-system/create-edit-whitelist-system.component";

@NgModule({
    declarations: [WhitelistSystemComponent, CreateEditWhitelistSystemComponent],
    imports: [
        CommonModule,
        FormsModule,
        SharedModule,
        WhitelistSystemRoutingModule,
        NgxPaginationModule
    ],
    entryComponents: [CreateEditWhitelistSystemComponent]
})
export class WhitelistSystemModule { 

}