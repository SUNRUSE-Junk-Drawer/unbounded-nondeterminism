using Akka.Actor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UnboundedNondeterminism
{
    /// <summary>Encapsulates all aspects of a ruleset.</summary>
    public sealed class Ruleset : PersistableBase
    {
        /// <summary>A request to create a new global variable.</summary>
        /// <remarks>A name will be automatically generated.</remarks>
        public sealed class CreateGlobalVariable { }

        /// <summary>Returned in response to <see cref="CreateGlobalVariable"/>.</summary>
        public sealed class GlobalVariableCreated
        {
            /// <summary>The <see cref="PersistableBase.PersistenceGuid"/> of the created global variable.</summary>
            public Guid Id;

            /// <summary>The name of the created global variable.</summary>
            public string Name;
        }

        /// <summary>A request to change the name of an existing global variable.</summary>
        public sealed class RenameGlobalVariable
        {
            /// <summary>The <see cref="PersistableBase.PersistenceGuid"/> of the global variable to rename.</summary>
            public Guid Id;

            /// <summary>The new name for the global variable.</summary>
            public string NewName;
        }

        /// <summary>Returned in response to <see cref="RenameGlobalVariable"/> when renaming was successful.</summary>
        public sealed class GlobalVariableRenamed { }

        /// <summary>Returned in response to <see cref="RenameGlobalVariable"/> when the global variable cannot be renamed as another global variable has the requested <see cref="RenameGlobalVariable.NewName"/>.</summary>
        public sealed class GlobalVariableCannotBeRenamedAsTheNewNameIsNotUnique { }

        /// <summary>Returned in response to <see cref="RenameGlobalVariable"/> when the global variable cannot be renamed as it has been deleted.</summary>
        public sealed class GlobalVariableCannotBeRenamedAsItWasDeleted { }

        /// <summary>Returned in response to <see cref="RenameGlobalVariable"/> when the global variable cannot be renamed as <see cref="RenameGlobalVariable.Id"/> refers to a nonexistent global variable <see cref="PersistableBase.PersistenceGuid"/>.</summary>
        public sealed class GlobalVariableCannotBeRenamedAsItWasNeverCreated { }

        /// <summary>A request to delete an existing global variable.</summary>
        public sealed class DeleteGlobalVariable
        {
            /// <summary>The <see cref="PersistableBase.PersistenceGuid"/> of the global variable to delete.</summary>
            public Guid Id;
        }

        /// <summary>Returned in response to <see cref="DeleteGlobalVariable"/> when deletion was successful.</summary>
        /// <remarks>This is also returned when the global variable had previously been deleted.</remarks>
        public sealed class GlobalVariableDeleted { }

        /// <summary>Returned in response to <see cref="DeleteGlobalVariable"/> when the global variable cannot be deleted as it is in use.</summary>
        public sealed class GlobalVariableCannotBeDeletedAsItIsInUse { }

        /// <summary>Returned in response to <see cref="DeleteGlobalVariable"/> when the global variable cannot be deleted as <see cref="DeleteGlobalVariable.Id"/> refers to a nonexistent global variable <see cref="PersistableBase.PersistenceGuid"/>.</summary>
        public sealed class GlobalVariableCannotBeDeletedAsItWasNeverCreated { }

        /// <summary>A request to get all existing global variables.</summary>
        public sealed class GetGlobalVariables { }

        /// <summary>Returned in response to <see cref="GetGlobalVariables"/>.</summary>
        public sealed class GotGlobalVariables
        {
            /// <summary>The <see cref="PersistableBase.PersistenceGuid"/> and name of every existing global variable.</summary>
            public IReadOnlyDictionary<Guid, string> GlobalVariables;
        }

        /// <inheritdoc />
        public Ruleset(Guid persistenceGuid) : base(persistenceGuid) { }
    }
}
