export class MenuItem {
    id: number;
    parentId: number;
    name = '';
    permissionName = '';
    icon = '';
    route = '';
    isActive?: boolean;
    isCollapsed?: boolean;
    children: MenuItem[];

    constructor(name: string, permissionName: string, icon: string, route: string, childItems: MenuItem[] = null) {
        this.name = name;
        this.permissionName = permissionName;
        this.icon = icon;
        this.route = route;
        this.children = childItems;
    }
}