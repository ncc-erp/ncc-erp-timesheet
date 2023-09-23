import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";
import { PositionRoutingModule } from "./position-routing.module";
import { PositionComponent } from "./position.component";
import { CreateEditPositionComponent } from "./create-edit-position/create-edit-position.component";
import { NgxPaginationModule } from "ngx-pagination";
import { NgxMatSelectSearchModule } from "ngx-mat-select-search";
import { SharedModule } from "@shared/shared.module";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";

@NgModule({
  declarations: [PositionComponent, CreateEditPositionComponent],
  imports: [
    CommonModule,
    SharedModule,
    ReactiveFormsModule,
    FormsModule,
    PositionRoutingModule,
    NgxPaginationModule,
    NgxMatSelectSearchModule,
  ],
  entryComponents: [CreateEditPositionComponent],
  exports: [CreateEditPositionComponent],
})
export class PositionModule {}
