import { UserGroupRole } from "../group-roles.interface";

export interface SetGroupRoleResponse {
    groupRole: UserGroupRole,
    userThatAssignedProductOwner: string,
    isSwitchingScrumMaster: boolean,
    userThatWasScrumMaster: string
    userThatIsScrumMaster: string
}