import { Component, Injector, OnInit, ViewEncapsulation } from "@angular/core";
import { GetTrackerWhitelistDto } from "@app/service/api/model/tracker-whitelist.dto";
import { PagedListingComponentBase } from "@shared/paged-listing-component-base";

@Component({
  selector: 'app-tracker-whitelist',
  templateUrl: './tracker-whitelist.component.html',
  styleUrls: ['./tracker-whitelist.component.css']
})
export class TrackerWhitelistComponent extends PagedListingComponentBase<GetTrackerWhitelistDto> implements OnInit {

  constructor(injector: Injector) {
    super(injector);
  }

  ngOnInit(): void {
  }

  protected list(request: any, pageNumber: number, finishedCallback: Function): void {
    throw new Error('Method not implemented.');
  }

  protected delete (item: GetTrackerWhitelistDto): void {
    throw new Error('Method not implemented.');
    }

}