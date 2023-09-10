import { Component, Injector, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material';
import { PERMISSIONS_CONSTANT } from '@app/constant/permission.constant';
import { GetUserAvatarPathDto, MyProfileDto } from '@app/service/api/model/my-profile-dto';
import { MyProfileService } from '@app/service/api/my-profile.service';
import { UserService } from '@app/service/api/user.service';
import { AppComponentBase } from '@shared/app-component-base';
import { AppSessionService } from '@shared/session/app-session.service';
import * as moment from 'moment';
import { UpdateUserProfileComponent } from './update-user-profile/update-user-profile.component';

@Component({
  selector: 'app-my-profile',
  templateUrl: './my-profile.component.html',
  styleUrls: ['./my-profile.component.css']
})
export class MyProfileComponent extends AppComponentBase implements OnInit {

  constructor(injector: Injector,
    private myProfileService : MyProfileService,
    public dialog: MatDialog,
    private sessionService: AppSessionService,
    private userService : UserService,
    ) {
    super(injector);
    this.currentLoginUserId = this.sessionService.userId;
  }

  public userProfile = {} as MyProfileDto;
  public sexEnum = SexEnumFromHRM;
  public currentLoginUserId: number = 0;
  public currentLoginUserEmail: string = "";
  public isFoundEmployeeProfile: boolean = false;
  public userAvatarPath = {} as GetUserAvatarPathDto;
  public isLoading:boolean = false;
  ngOnInit() {
    this.onRefresh();
  }

  public getUserEmailById(){
    this.userService.getUserEmailById(this.currentLoginUserId).subscribe((rs)=>{
      this.currentLoginUserEmail = rs.result;
      this.getUserInfoByEmail(rs.result);
    })
  }

  public getUserAvatarById(){
    this.userService.getUserAvatarById(this.currentLoginUserId).subscribe((rs)=>{
      this.userAvatarPath = rs.result;
    })
  }

  public getUserInfoByEmail(email){
    this.isLoading = true;
    this.myProfileService.getUserInfoByEmail(email).subscribe((rs)=>{
      this.userProfile = rs.result;
      if(this.userProfile != null){
        this.isFoundEmployeeProfile = true;
      }
      this.isLoading = false;
    })
  }


  public onUpdate(userProfile){
    if(userProfile.issuedOn){
      userProfile.issuedOn =  moment(userProfile.issuedOn).format("YYYY-MM-DD")
    }
    const dialog = this.dialog.open(UpdateUserProfileComponent,{
      data : {email: userProfile.email 
        , bankId : userProfile.bankId},
      width: "750px",
      maxHeight: "99vh"
    });
    dialog.afterClosed().subscribe((rs)=>{
      if(rs){
        if(rs.isSucess){
          abp.notify.success(rs.resultMessage);
        }else{
          abp.notify.error(rs.resultMessage);
        }
        this.getUserInfoByEmail(this.currentLoginUserEmail);
      }
    })
  }
  public getAvatar(avatar) {
    if (avatar.avatarFullPath) {
      return avatar.avatarFullPath;
    }
    if (avatar.sex == 1) {
      return 'assets/images/women.png';
    }
    return 'assets/images/men.png';
  }

  public onRefresh(){
    this.getUserEmailById();
    this.getUserAvatarById();
  }

  public isShowRequestUpdateInfoBtn(){
    return this.isGranted(PERMISSIONS_CONSTANT.RequestUpdateInfo)
  }

}
export enum SexEnumFromHRM{
  Male = 1,
  Female = 2
}
