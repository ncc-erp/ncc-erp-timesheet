export class NewsDto {
        title: string;
        content: string;
        isAllowComment: boolean;
        creationTime: string;
        creatorUserId: number;
        userName: string;
        id: number;
        comments: GetCommentDto[];
}

export class GetCommentDto {
        content: String;
        commentID: number;
        postID: number;
        creationTime: String;
        userName: String;
        creatorUserId: number;
        id: number;
}
export class CommentDto {
        content: string;
        commentId: number;
        postId: number;
        id: number;
        isEditing: boolean;
}
export class newsdto {
        title: string;
        content: any;
        isAllowComment: boolean;
        creationTime: string;
        creatorUserId: number;
        userName: string;
        id: number;
      }
