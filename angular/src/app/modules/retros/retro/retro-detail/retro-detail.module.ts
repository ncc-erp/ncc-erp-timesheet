import { NgModule } from "@angular/core";
import { CommonModule } from "@angular/common";

import { RetroDetailRoutingModule } from "./retro-detail-routing.module";
import { RetroDetailComponent } from "./retro-detail.component";
import { SharedModule } from "@shared/shared.module";
import { FormsModule, ReactiveFormsModule } from "@angular/forms";
import { NgxPaginationModule } from "ngx-pagination";
import { NgxMatSelectSearchModule } from "ngx-mat-select-search";
import { ImportRetroDetailComponent } from "./import-retro-detail/import-retro-detail.component";
import { NgxSliderModule } from "@angular-slider/ngx-slider";
import { CreateEditRetroDetailComponent } from "./create-edit-retro-detail/create-edit-retro-detail.component";

@NgModule({
  declarations: [
    RetroDetailComponent,
    ImportRetroDetailComponent,
    CreateEditRetroDetailComponent,
  ],
  imports: [
    CommonModule,
    SharedModule,
    ReactiveFormsModule,
    FormsModule,
    RetroDetailRoutingModule,
    NgxPaginationModule,
    NgxMatSelectSearchModule,
    NgxSliderModule,
    SharedModule,
  ],
  entryComponents: [ImportRetroDetailComponent, CreateEditRetroDetailComponent],
})
export class RetroDetailModule {}
