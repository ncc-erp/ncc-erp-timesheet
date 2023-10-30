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
import { GenerateDataComponent } from "./generate-data/generate-data.component";
import { AddMultiRetroDetailComponent } from './add-multi-retro-detail/add-multi-retro-detail.component';
import { AppAddUserOtherProjectRetroResultComponent } from './app-add-user-other-project-retro-result/app-add-user-other-project-retro-result.component';

@NgModule({
  declarations: [
    RetroDetailComponent,
    ImportRetroDetailComponent,
    CreateEditRetroDetailComponent,
    GenerateDataComponent,
    AddMultiRetroDetailComponent,
    AppAddUserOtherProjectRetroResultComponent,
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
  entryComponents: [ImportRetroDetailComponent, CreateEditRetroDetailComponent, GenerateDataComponent, AddMultiRetroDetailComponent],
})
export class RetroDetailModule {}
