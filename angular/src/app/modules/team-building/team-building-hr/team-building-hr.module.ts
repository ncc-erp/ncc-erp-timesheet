import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { ReactiveFormsModule, FormsModule } from "@angular/forms";
import { SharedModule } from "@shared/shared.module";
import { NgxMatSelectSearchModule } from "ngx-mat-select-search";
import { NgxPaginationModule } from "ngx-pagination";
import { TeamBuildingHRRoutingModule } from "./team-building-hr-routing.module";
import { TeamBuildingHrComponent } from "./team-building-hr.component";
import { GenerateDataComponent } from './generate-data/generate-data.component';
import { AddEmployeeComponent } from './add-employee/add-employee.component';
import { EditMoneyComponent } from './edit-money/edit-money.component';
import { NgxCurrencyModule } from "ngx-currency";
// import { CreateTeambuildingDetailComponent } from './create-teambuilding-detail/create-teambuilding-detail.component';

@NgModule({
    declarations: [TeamBuildingHrComponent, GenerateDataComponent, AddEmployeeComponent, EditMoneyComponent, ],
    imports: [
      CommonModule,
      SharedModule,
      ReactiveFormsModule,
      FormsModule,
      TeamBuildingHRRoutingModule,
      NgxPaginationModule,
      NgxMatSelectSearchModule,
      NgxCurrencyModule,
    ],
    entryComponents: [
      GenerateDataComponent,
      AddEmployeeComponent,
      EditMoneyComponent
      // CreateTeambuildingDetailComponent
    ],
    exports: [

    ]
  })
export class TeamBuildingHRModule { }
