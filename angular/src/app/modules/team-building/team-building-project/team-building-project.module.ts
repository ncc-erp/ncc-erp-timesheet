import { CommonModule } from "@angular/common";
import { NgModule } from "@angular/core";
import { ReactiveFormsModule, FormsModule } from "@angular/forms";
import { SharedModule } from "@shared/shared.module";
import { NgxMatSelectSearchModule } from "ngx-mat-select-search";
import { NgxPaginationModule } from "ngx-pagination";
import { TeamBuildingProjectRoutingModule } from "./team-building-project-routing.module";
import { TeamBuildingProjectComponent } from "./team-building-project.component";

@NgModule({
    declarations: [TeamBuildingProjectComponent],
    imports: [
      CommonModule,
      SharedModule,
      ReactiveFormsModule,
      FormsModule,
      TeamBuildingProjectRoutingModule,
      NgxPaginationModule,
      NgxMatSelectSearchModule
    ],
    entryComponents: [
    ],
    exports: [

    ]
  })
export class TeamBuildingProjectModule { }
