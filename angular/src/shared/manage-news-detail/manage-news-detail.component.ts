import { Component, OnInit, Injector, Input, OnDestroy } from '@angular/core';
import { ManageNewsService } from '@app/service/api/manage-new.service';
import { NewsDto, GetCommentDto } from '@app/service/api/model/news-dto';
import { ActivatedRoute } from '@angular/router';
import { CommentService } from '@app/service/api/comment.service';
import { AppComponentBase } from '@shared/app-component-base';
import * as _ from 'lodash';
import { AuthService } from 'angularx-social-login';
import { AppSessionService } from '@shared/session/app-session.service';

@Component({
    selector: 'app-manage-news-detail',
    templateUrl: './manage-news-detail.component.html',
    styleUrls: ['./manage-news-detail.component.css']
})
export class ManageNewsDetailComponent extends AppComponentBase implements OnInit {
    @Input() id: number;
    private postID: String;
    item = {} as GetCommentDto;
    news = {} as NewsDto;
    comment = [];
    content: string;
    contentReply: String;
    reply = [] as GetCommentDto[];
    isRely: boolean = false;
    isshowRely: boolean = false;
    isCancel: boolean = true;
    isLoading: boolean;
     interval: any;
    constructor(private route: ActivatedRoute,
        injector: Injector,
        private manageNewsService: ManageNewsService,
        private commentService: CommentService,
        public auth: AuthService,
        private sessionService: AppSessionService,
    ) {

        super(injector);
    }
    ngOnInit() {
        this.route.params.subscribe(params => {
            this.postID = params['id'];
        });
        this.getNews();
        this.interval = setInterval(() => { 
            this.manageNewsService.get(this.postID).subscribe(res => {
                const comment = this.buildData(res.result.comments).map(el => {
                    const _el: any = el;
                    _el.isComment = false;
                    _el.isAnswer = false;
                    return _el;
                });
                const _comment = this.upDatedata(this.comment, comment);
                this.comment = [..._comment];
            }) 
        },60000);   
    }
    // update comment mà không phải load lại trang   khi comment 2 tài khoản cùng lúc
    upDatedata(cur: any[],next: any[]) {
        if(!next) {
          return null;
        }
        return next.map(el => {
            const _el: any = el;
            const data = cur.find(e => e.id == _el.id);
            if (data == null) {
                _el.isComment = false;
                _el.isAnswer = false;
            } else {
                _el.isComment = data.isComment;
                _el.isAnswer = data.isAnswer;
                _el.isEditing= data.isEditing ? data.isEditing : false;
                _el.isEditingReply= data.isEditingReply ? data.isEditingReply : false;
                _el.ismenu= data.ismenu ? data.ismenu : false;
                // console.log(data.reply,)
                _el.reply = this.upDatedata(data['reply'], _el['reply']);               
            }
            return _el;
        });
        
            
    }

    getNews(): void {
        // this.isLoading =  true;
        this.manageNewsService.get(this.postID).subscribe(res => {
            this.news = res.result
            this.comment = this.buildData(this.news.comments).map(el => {
                const _el: any = el;
                _el.isComment = false;
                _el.isAnswer = false;              
                // this.isLoading = false;
                return _el;
                
            });

        })
    }

    showRely(item) {
        item.isComment = !item.isComment;
    }
    reLy(item) {
        item.isAnswer = !item.isAnswer;
        this.contentReply = '';
    }
    buildData(data: GetCommentDto[]) {
        return _(data)
            .filter(el => el.commentID == null)
            .groupBy(x => x.id)
            .map((value, key) => ({
                id: value[0].id,
                content: value[0].content,
                creationTime: value[0].creationTime,
                userName: value[0].userName,
                commentID: value[0].commentID,
                postId: value[0].postID,
                creatorUserId: value[0].creatorUserId,
                reply: data.filter(x => x.commentID === value[0].id,)
            })).value();
    }
   editComment(item) {
        if (item.isEditing) {
            item.isEditing = false;
        } else {
            item.isEditing = true;
        }
    }
    // rely: any = {};
    editCommentRely(reply) {
        if (reply.isEditingReply) {
            reply.isEditingReply = false;
            reply.ismenu = false;
        } else {
            reply.isEditingReply = true;
            reply.ismenu = true;

        }
      
    }
    cancel() {
        this.getNews();
    }

    goBack() {
        window.history.back();
    }

    onCreate(content, parentId: number = null) {
        const data: any = {
            id: 0,
            content: content.trim(),
            postId: Number(this.postID),
            commentId: parentId == null ? null : parentId
        };
        this.onSubmit(data);
    }

    onEdit(data) {
        this.onSubmit(data, true);
    }

    onDelete(child, parent: any = null) {
        abp.message.confirm(
            "Delete comment: '" + child.content + "'?",
            (result: boolean) => {
                if (result) {
                    this.commentService.delete(child.id).subscribe(res => {
                        this.notify.success(this.l('Comment deleted '));
                        if (parent == null) {
                            this.comment.splice(this.comment.indexOf(child), 1);
                        } else {
                            this.comment.splice(this.comment[this.comment.indexOf(parent)].reply.find(r => r.id == child.id), 1);
                            this.getNews();
                        }
                       
                    });
                }
            })
    }

    onSubmit(data, isEdit: Boolean = false) {
        this.commentService.save(data).subscribe(r => {
            this.content = '';
            this.notify.success(this.l((isEdit == false ? 'create ' : 'edit ')) + 'Comment Successfully');
            this.getNews();
        })
    }

}