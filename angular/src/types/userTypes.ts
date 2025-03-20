export interface IMezonUser {
    email: string;
    mezon_id: string;
    user: {
        avatar_url: string;
        display_name: string;
        id: string;
        username: string;
    };
    wallet: string;
}
