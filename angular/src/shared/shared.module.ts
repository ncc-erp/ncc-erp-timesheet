import { FormatDisplayNumberToK } from './pipes/formatDisplayNumberToK.pipe';
import { ReactiveFormsModule, FormsModule } from "@angular/forms";
import { CommonModule } from "@angular/common";
import { NgModule, ModuleWithProviders } from "@angular/core";
import { AbpModule } from "@abp/abp.module";
import { RouterModule } from "@angular/router";
import { NgxPaginationModule } from "ngx-pagination";

import { AppSessionService } from "./session/app-session.service";
import { AppUrlService } from "./nav/app-url.service";
import { AppAuthService } from "./auth/app-auth.service";
import { AppRouteGuard } from "./auth/auth-route-guard";
import { AbpPaginationControlsComponent } from "./pagination/abp-pagination-controls.component";
import { LocalizePipe } from "@shared/pipes/localize.pipe";
import { DragDropModule } from "@angular/cdk/drag-drop";
import { ScrollingModule } from "@angular/cdk/scrolling";
import { CdkTableModule } from "@angular/cdk/table";
import { CdkTreeModule } from "@angular/cdk/tree";
import {
  MatAutocompleteModule,
  MatBadgeModule,
  MatBottomSheetModule,
  MatButtonModule,
  MatButtonToggleModule,
  MatCardModule,
  MatCheckboxModule,
  MatChipsModule,
  MatDatepickerModule,
  MatDialogModule,
  MatDividerModule,
  MatExpansionModule,
  MatGridListModule,
  MatIconModule,
  MatInputModule,
  MatListModule,
  MatMenuModule,
  MatNativeDateModule,
  MatPaginatorModule,
  MatProgressBarModule,
  MatProgressSpinnerModule,
  MatRadioModule,
  MatRippleModule,
  MatSelectModule,
  MatSidenavModule,
  MatSliderModule,
  MatSlideToggleModule,
  MatSnackBarModule,
  MatSortModule,
  MatStepperModule,
  MatTableModule,
  MatTabsModule,
  MatToolbarModule,
  MatTooltipModule,
  MatTreeModule,
} from "@angular/material";
import { BlockDirective } from "./directives/block.directive";
import { BusyDirective } from "./directives/busy.directive";
import { EqualValidator } from "./directives/equal-validator.directive";
import { CommonInputModule } from "./common-input/common-input.module";
import { TextMaskModule } from "angular2-text-mask";
import { UploadComponent } from "./upload/upload.component";
//
import { InfiniteScrollModule } from "ngx-infinite-scroll";
import { ManageNewsDetailComponent } from "./manage-news-detail/manage-news-detail.component";
import { NoPermissionComponent } from "./interceptor-errors/no-permission/no-permission.component";
import { DateSelectorComponent } from "./date-selector/date-selector.component";
import { DateSelectorNewComponent } from "./date-selector-new/date-selector-new.component";
import { PopupComponent } from "./date-selector/popup/popup.component";
import { LevelPipe } from "./pipes/level.pipe";
import { SortableComponent } from "./sortable/sortable.component";
import { MultipleSelectComponent } from "./multiple-select/multiple-select.component";
import { MultipleSelectNewComponent } from "./multiple-select-new/multiple-select-new.component";
import { SafeHtmlPipe } from './pipes/safeHtml.pipe';
import { ResizeCellDirective } from './directives/resizeCell.directive';
import { SortableHeaderComponent } from './sortable-header/sortable-header.component';
import { PunishNamePipe } from './pipes/punishName.pipe';
import { DayTypePipe } from './pipes/dateType.pipe';
import { AddUserOtherProjectComponent } from './add-user-other-project/add-user-other-project.component';
@NgModule({
  imports: [
    FormsModule,
    CommonModule,
    AbpModule,
    RouterModule,
    NgxPaginationModule,
    MatAutocompleteModule,
    MatBadgeModule,
    MatBottomSheetModule,
    MatButtonModule,
    MatButtonToggleModule,
    MatCardModule,
    MatCheckboxModule,
    MatChipsModule,
    MatDatepickerModule,
    MatDialogModule,
    MatDividerModule,
    MatExpansionModule,
    MatGridListModule,
    MatIconModule,
    MatInputModule,
    MatListModule,
    MatMenuModule,
    MatNativeDateModule,
    MatPaginatorModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatRadioModule,
    MatRippleModule,
    MatSelectModule,
    MatSidenavModule,
    MatSliderModule,
    MatSlideToggleModule,
    MatSnackBarModule,
    MatSortModule,
    MatStepperModule,
    MatTableModule,
    MatTabsModule,
    MatToolbarModule,
    MatTooltipModule,
    MatTreeModule,
    CommonInputModule,
    TextMaskModule,
    InfiniteScrollModule,
    ReactiveFormsModule,
  ],
  declarations: [
    AbpPaginationControlsComponent,
    LocalizePipe,
    BlockDirective,
    BusyDirective,
    EqualValidator,
    UploadComponent,
    ManageNewsDetailComponent,
    NoPermissionComponent,
    DateSelectorComponent,
    DateSelectorNewComponent,
    PopupComponent,
    LevelPipe,
    SortableComponent,
    MultipleSelectComponent,
    MultipleSelectNewComponent,
    SafeHtmlPipe,
    ResizeCellDirective,
    SortableHeaderComponent,
    FormatDisplayNumberToK,
    PunishNamePipe,
    DayTypePipe,
    AddUserOtherProjectComponent,
  ],
  exports: [
    AbpPaginationControlsComponent,
    LocalizePipe,
    BlockDirective,
    BusyDirective,
    EqualValidator,
    CdkTableModule,
    CdkTreeModule,
    DragDropModule,
    MatAutocompleteModule,
    MatBadgeModule,
    MatBottomSheetModule,
    MatButtonModule,
    MatButtonToggleModule,
    MatCardModule,
    MatCheckboxModule,
    MatChipsModule,
    MatStepperModule,
    MatDatepickerModule,
    MatDialogModule,
    MatDividerModule,
    MatExpansionModule,
    MatGridListModule,
    MatIconModule,
    MatInputModule,
    MatListModule,
    MatMenuModule,
    MatNativeDateModule,
    MatPaginatorModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatRadioModule,
    MatRippleModule,
    MatSelectModule,
    MatSidenavModule,
    MatSliderModule,
    MatSlideToggleModule,
    MatSnackBarModule,
    MatSortModule,
    MatTableModule,
    MatTabsModule,
    MatToolbarModule,
    MatTooltipModule,
    MatTreeModule,
    ScrollingModule,
    CommonInputModule,
    ReactiveFormsModule,
    TextMaskModule,
    UploadComponent,
    InfiniteScrollModule,
    ManageNewsDetailComponent,
    DateSelectorComponent,
    DateSelectorNewComponent,
    LevelPipe,
    SortableComponent,
    MultipleSelectComponent,
    MultipleSelectNewComponent,
    SafeHtmlPipe,
    ResizeCellDirective,
    SortableHeaderComponent,
    FormatDisplayNumberToK,
    PunishNamePipe,
    DayTypePipe,
    AddUserOtherProjectComponent
  ],
  entryComponents: [
    PopupComponent,
  ],
})
export class SharedModule {
  static forRoot(): ModuleWithProviders {
    return {
      ngModule: SharedModule,
      providers: [
        AppSessionService,
        AppUrlService,
        AppAuthService,
        AppRouteGuard,
      ],
    };
  }
}
