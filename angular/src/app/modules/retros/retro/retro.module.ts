import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";

import { RetroRoutingModule } from "./retro-routing.module";
import { RetroComponent } from "./retro.component";
import { SharedModule } from "@shared/shared.module";
import { CreateEditRetroComponent } from "./create-edit-retro/create-edit-retro.component";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { NgxMatSelectSearchModule } from "ngx-mat-select-search";
import { NgxPaginationModule } from "ngx-pagination";
@NgModule({
  declarations: [RetroComponent, CreateEditRetroComponent],
  imports: [
    CommonModule,
    RetroRoutingModule,
    NgxPaginationModule,
    FormsModule,
    SharedModule,
    ReactiveFormsModule,
    NgxMatSelectSearchModule,
  ],
  entryComponents: [CreateEditRetroComponent],
  exports: [CreateEditRetroComponent],
})
export class RetroModule {}
