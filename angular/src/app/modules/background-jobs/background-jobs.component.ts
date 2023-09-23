import { Component, Injector, OnInit } from '@angular/core';
import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
import { BackgroundJobService } from '@app/service/api/background-job.service';
import { GetAllBackgroundJobsDto, InputToGetAllDto } from '@app/service/api/model/background-job-dto';
import { appModuleAnimation } from '@shared/animations/routerTransition';
import { SortDirectionEnum } from '@shared/AppEnums';
import { PagedListingComponentBase, PagedRequestDto } from '@shared/paged-listing-component-base';
import { SortableModel } from '@shared/sortable/sortable.component';
import { finalize } from 'rxjs/operators';

@Component({
  selector: 'app-background-jobs',
  templateUrl: './background-jobs.component.html',
  styleUrls: ['./background-jobs.component.css'],
  animations: [appModuleAnimation()]
})
export class BackgroundJobsComponent extends PagedListingComponentBase<any> implements OnInit {

  protected list(request: PagedRequestDto, pageNumber: number, finishedCallback: Function): void {
    let input = {
      param : request,
      searchById: this.searchById
    } as  InputToGetAllDto;

    this.isTableLoading = true;
    this.backgroundJobService
      .getAllBackgroundJob(input)
      .pipe(finalize(() => {
        finishedCallback();
      }))
      .subscribe((result: any) => {
        this.listBackgroundJobs = result.result.items;
        this.showPaging(result.result, pageNumber);
        this.isTableLoading = false;
      },()=> this.isTableLoading = false);
  }
  protected delete(id): void {
    abp.message.confirm("Are you sure to delete this job?", "", (rs)=>{
      if(rs){
        this.backgroundJobService.delete(id).subscribe((rs)=>{
          if(rs){
            abp.message.success("Delete job successful");
            this.refresh();
          }
        })
      }
    })
  }

  constructor( injector: Injector, private backgroundJobService: BackgroundJobService) {
    super(injector);
  }

  public listBackgroundJobs:GetAllBackgroundJobsDto[] = [];
  public searchById:string = "";
  public sortDirectionEnum = SortDirectionEnum;
  ngOnInit() {
    this.defaultSoft();
    this.refresh();
  }
  onSearchById(){
    this.getDataPage(1);
  }
  defaultSoft(){
    this.sortProperty = 'creationTime';
    this.sortDirection = this.sortDirectionEnum.Descending;
  }
  isShowDeleteBtn(){
    return this.isGranted(PERMISSIONS_CONSTANT.Admin_BackgroundJob_Delete);
  }
 

}
