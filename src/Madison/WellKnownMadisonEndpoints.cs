// Copyright (C) 2026 Canonical Ltd.
// 
// SPDX-License-Identifier: GPL-3.0-only
// 
// This program is free software: you can redistribute it and/or modify it under the terms of
// the GNU General Public License version 3, as published by the Free Software Foundation.
// 
// This program is distributed in the hope that it will be useful, but WITHOUT ANY
// WARRANTY; without even the implied warranties of MERCHANTABILITY, SATISFACTORY
// QUALITY, or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU General Public License
// for more details.
// 
// You should have received a copy of the GNU General Public License along with this
// program.  If not, see http://www.gnu.org/licenses/.

namespace Canonical.Madison;

public static class WellKnownMadisonEndpoints
{   
    /// <summary>
    /// Primary/Default Madison endpoint for the Ubuntu project. 
    /// </summary>
    public static readonly Uri Ubuntu = new("https://ubuntu-archive-team.ubuntu.com/madison.cgi");
    
    /// <summary>
    /// Primary/Default Madison endpoint for the Debian project. 
    /// </summary>
    public static readonly Uri Debian = new("https://api.ftp-master.debian.org/madison");
    
    /// <summary>
    /// Madison endpoint for the upload queue of the Debian project.
    /// </summary>
    /// <remarks>
    /// The Uri of this endpoint sets the suite to 'new'. Do NOT specify a suite yourself when using this
    /// endpoint for it to work properly.
    /// </remarks>
    /// <seealso href="https://ftp-master.debian.org/new.html"/>
    public static readonly Uri DebianNew = new("https://api.ftp-master.debian.org/madison?s=new");
    
    /// <summary>
    /// Madison endpoint of the Debian Quality Assurance (QA) Team.
    /// </summary>
    public static readonly Uri DebianQa = new("https://qa.debian.org/madison.php");

    /// <summary>
    /// Madison endpoint of the Debian Quality Assurance (QA) Team for upload queue of the Debian project.
    /// </summary>
    /// <seealso href="https://ftp-master.debian.org/new.html"/>
    public static readonly Uri DebianQaNew = new("https://qa.debian.org/cgi-bin/madison.cgi?table=new");
    
    /// <summary>
    /// Madison endpoint of the Debian Quality Assurance (QA) Team for ports of Debian.
    /// </summary>
    public static readonly Uri DebianQaPorts = new("https://qa.debian.org/cgi-bin/madison.cgi?table=ports");
    
    /// <summary>
    /// Madison endpoint of the Debian Quality Assurance (QA) Team for archived releases of Debian.
    /// </summary>
    public static readonly Uri DebianQaArchived = new("https://qa.debian.org/cgi-bin/madison.cgi?table=archived");
    
    /// <summary>
    /// Madison endpoint of the Debian Quality Assurance (QA) Team for the "all" table.
    /// </summary>
    public static readonly Uri DebianQaAll = new("https://qa.debian.org/cgi-bin/madison.cgi?table=all");
    
    /// <summary>
    /// Madison endpoint of the Debian Quality Assurance (QA) Team for Ubuntu releases.
    /// </summary>
    public static readonly Uri DebianQaUbuntu = new("https://qa.debian.org/cgi-bin/madison.cgi?table=ubuntu");
    
    /// <summary>
    /// Madison endpoint of the Debian janitor project. 
    /// </summary>
    public static readonly Uri DebianJanitor = new("https://janitor.debian.net/api/madison");
    
    /// <summary>
    /// Madison endpoint of the Ultimate Debian Database project.
    /// </summary>
    public static readonly Uri UltimateDebianDatabase = new("https://qa.debian.org/cgi-bin/madison.cgi");
}
