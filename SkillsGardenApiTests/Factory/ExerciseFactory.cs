using SkillsGardenApi.Models;
using SkillsGardenDTO;

namespace SkillsGardenApiTests.Factory
{
    public static class ExerciseFactory
    {
        public static Exercise CreateExercise(string name = "Touwtje springen")
        {
            return new Exercise
            {
                Name = name,
                Media = "https://asm.nl/video/touwtje-springen"
            };
        }

        public static ExerciseRequirement CreateExerciseRequirement(string requirement)
        {
            return new ExerciseRequirement
            {
                Requirement = requirement
            };
        }

        public static ExerciseStep CreateExerciseStep(int number, string description)
        {
            return new ExerciseStep
            {
                StepNumber = number,
                StepDescription = description
            };
        }

        public static ExerciseForm CreateExerciseForm(MovementForm form)
        {
            return new ExerciseForm
            {
                MovementForm = form
            };
        }
    }
}
