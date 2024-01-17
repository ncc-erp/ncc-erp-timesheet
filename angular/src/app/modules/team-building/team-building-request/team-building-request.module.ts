import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { ReactiveFormsModule, FormsModule } from "@angular/forms";
import { MatDialogModule } from "@angular/material";
import { SharedModule } from "@shared/shared.module";
import { NgxCurrencyModule } from "ngx-currency";
import { NgxMatSelectSearchModule } from "ngx-mat-select-search";
import { NgxPaginationModule } from "ngx-pagination";
import { DisburseRequestComponent } from "./disburse-request/disburse-request.component";
import { TeamBuildingRequestRoutingModule } from "./team-building-request-routing.module";
import { TeamBuildingRequestComponent } from "./team-building-request.component";
import { RequestDetailComponent } from './request-detail/request-detail.component';
import { EditRequestComponent } from './edit-request/edit-request.component';
import { EditInvoiceMoneyComponent } from './disburse-request/edit-invoice-money/edit-invoice-money.component';

@NgModule({
    declarations: [TeamBuildingRequestComponent, DisburseRequestComponent, RequestDetailComponent, EditRequestComponent, EditInvoiceMoneyComponent],
    imports: [
      CommonModule,
      SharedModule,
      ReactiveFormsModule,
      FormsModule,
      TeamBuildingRequestRoutingModule,
      NgxPaginationModule,
      NgxMatSelectSearchModule,
      NgxCurrencyModule,
      MatDialogModule
    ],
    entryComponents: [
      DisburseRequestComponent,
      RequestDetailComponent,
      EditRequestComponent,
      EditInvoiceMoneyComponent
    ],
    exports: [

    ]
  })
export class TeamBuildingRequestModule { }
