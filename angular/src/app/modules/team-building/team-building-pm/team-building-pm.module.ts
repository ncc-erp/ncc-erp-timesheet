import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { ReactiveFormsModule, FormsModule } from "@angular/forms";
import { SharedModule } from "@shared/shared.module";
import { NgxMatSelectSearchModule } from "ngx-mat-select-search";
import { NgxPaginationModule } from "ngx-pagination";
import { TeamBuildingPMRoutingModule } from "./team-building-pm-routing.module";
import { TeamBuildingPmComponent } from "./team-building-pm.component";
import { PmSendRequestComponent } from './pm-send-request/pm-send-request.component';
import { NgxCurrencyModule } from "ngx-currency";

@NgModule({
    declarations: [TeamBuildingPmComponent, PmSendRequestComponent],
    imports: [
      CommonModule,
      SharedModule,
      ReactiveFormsModule,
      FormsModule,
      TeamBuildingPMRoutingModule,
      NgxPaginationModule,
      NgxMatSelectSearchModule,
      NgxCurrencyModule,
    ],
    entryComponents: [
      PmSendRequestComponent
    ],
    exports: [

    ]
  })
export class TeamBuildingPMModule { }
