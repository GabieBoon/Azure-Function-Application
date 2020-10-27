using System.ComponentModel;

namespace SkillsGardenDTO.Error
{
    public enum ErrorCode
    {
        /**
         * 200 Status Code
         */
        [Description("OK")]
        OK = 200_1,

        /**
         * 400 Status Code
         */
        [Description("The request body you provided was invalid/incomplete")]
        INVALID_REQUEST_BODY = 400_1,
        [Description("The path parameter(s) you provided was/were invalid")]
        INVALID_PATH_PARAMETER = 400_2,
        [Description("The query parameter(s) you provided was/were invalid")]
        INVALID_QUERY_PARAMETER = 400_3,
        // USERS
        [Description("You can only view your own account")]
        CAN_ONLY_VIEW_OWN_ACCOUNT = 400_4,
        [Description("You can only edit your own account")]
        CAN_ONLY_EDIT_OWN_ACCOUNT = 400_5,
        // LOCATIONS
        [Description("You can only upload .png .jpg .jpeg images")]
        INVALID_IMAGE_FORMAT = 400_6,
        [Description("Image is to big: images can't be larger then 10MB")]
        IMAGE_FORMAT_BIGGER_THAN_10MB = 400_7,
        [Description("Name lenght is too long: name can be 50 characters long")]
        LOCATIONNAME_TOO_LONG = 400_8,
        [Description("Name lenght is too short: minimal length of name is 2 characters")]
        LOCATIONNAME_TOO_SHORT = 400_9,
        [Description("City lenght is too long: name can be 50 characters long")]
        LOCATIONCITY_TOO_LONG = 400_10,
        [Description("City lenght is too short: minimal length of city is 2 characters")]
        LOCATIONCITY_TOO_SHORT = 400_11,
        [Description("expected double, got a string")]
        INVALID_DOUBLEINPUT = 400_12,
        // WORKOUT
        [Description("Invalid movement form(s) provided")]
        INVALID_MOVEMENT_FORM_PROVIDED = 400_13,
        [Description("Invalid exercise provided")]
        INVALID_EXERCISE_PROVIDED = 400_14,
        // EVENTS
        [Description("You cannot register for an event you created")]
        CANNOT_REGISTER_FOR_OWN_EVENT = 400_15,
        [Description("The event registration limit has been reached")]
        EVENT_REGISTRATION_LIMIT_REACHED = 400_16,
        [Description("You are already registered for this event")]
        EVENT_ALREADY_REGISTERED = 400_17,
        // BEACONSLOG
        [Description("You can't log someone else")]
        ONLY_LOG_YOURSELF = 400_17,
        [Description("You can only get your own logs")]
        GET_ONLY_LOG_YOURSELF = 400_19,
        [Description("You can only delete your own logs")]
        ONLY_DELETE_OWN_LOGS = 400_20,
        // REGISTRATIONS
        [Description("You can only view your own registrations")]
        CAN_ONLY_VIEW_OWN_REGISTRATIONS = 400_21,

        /**
         * 401 Status Code
         */
        [Description("Bearer token is missing")]
        UNAUTHORIZED_BEARER_TOKEN_MISSING = 401_1,
        [Description("Bearer token is invalid")]
        UNAUTHORIZED_BEARER_TOKEN_INVALID = 401_2,
        [Description("You are not authorized to perform this operation")]
        UNAUTHORIZED_ROLE_NO_PERMISSIONS = 401_3,
        // LOGIN
        [Description("Combination of email and password is incorrect")]
        INVALID_COMBINATION_OF_EMAIL_AND_PASSWORD = 401_4,
        // USERS
        [Description("You are not authorized to set an user type")]
        UNAUTHORIZED_TO_SET_USER_TYPE = 401_5,
        [Description("You are not authorized to delete this user")]
        UNAUTHORIZED_TO_DELETE_USER = 401_6,

        /**
         * 404 Status Code
         */
        // USERS
        [Description("User not found")]
        USER_NOT_FOUND = 404_1,
        // LOCATIONS
        [Description("Location not found")]
        LOCATION_NOT_FOUND = 404_2,
        // EVENTS
        [Description("Event not found")]
        EVENT_NOT_FOUND = 404_3,
        [Description("Unable to delete event")]
        EVENT_DELETE_FAILED = 404_4,
        [Description("Event registration was not found")]
        EVENT_REGISTRATION_NOT_FOUND = 404_5,
        // COMPONENTS
        [Description("Component not found")]
        COMPONENT_NOT_FOUND = 404_6,
        [Description("Component image not found")]
        COMPONENT_IMAGE_NOT_FOUND = 404_7,
        [Description("Unable to delete component")]
        COMPONENT_DELETE_FAILED = 404_8,
        // EXERCISES
        [Description("Exercise not found")]
        EXERCISE_NOT_FOUND = 404_9,
        // WORKOUTS
        [Description("Workout not found")]
        WORKOUT_NOT_FOUND = 404_10,
        // BEACONS
        [Description("Beacon not found")]
        BEACON_NOT_FOUND = 404_11,
    }
}
